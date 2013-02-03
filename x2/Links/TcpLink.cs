// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using x2.Events;
using x2.Flows;
using x2.Queues;

namespace x2.Links {
  public abstract class TcpLink : SingleThreadedFlow {
    protected const byte sentinel = 0x55;
    protected Socket socket;

    protected TcpLink() : base(new UnboundedQueue<Event>()) {}

    public void Close() {
      if (socket != null) {
        socket.Close();
        //socket = null;
      }
    }

    public void OnReceive(IAsyncResult asyncResult) {
      AsyncRxState asyncState = (AsyncRxState)asyncResult.AsyncState;
      try {
        int numBytes = asyncState.Session.socket.EndReceive(asyncResult);

        if (numBytes > 0) {
          asyncState.Buffer.Stretch(numBytes);

          if (asyncState.beginning) {
            asyncState.Buffer.Rewind();
            int payloadLength;
            int numLengthBytes = asyncState.Buffer.ReadUInt29(out payloadLength);
            asyncState.Buffer.Shrink(numLengthBytes);
            asyncState.length = payloadLength;
          }

          if (asyncState.Buffer.Length < asyncState.length) {
            asyncState.Session.BeginReceive(false);
            return;
          }

          // end sentinel check
          if (asyncState.Buffer[asyncState.length - 1] != sentinel) {
            // protocol format error
          }

          // pre-process

          int typeId;
          asyncState.Buffer.ReadUInt29(out typeId);
          Event e = Event.Create(typeId);
          e.Load(asyncState.Buffer);
          e.handle = asyncState.Session.socket.Handle.ToInt64();
          PublishAway(e);

          asyncState.Buffer.Trim();
          asyncState.Session.BeginReceive(true);
        }
        else {
          // connection reset by peer
          LinkDisconnectedEvent e = new LinkDisconnectedEvent();
          e.Context = asyncState.Session;
          Feed(e);
        }
      }
      catch (SocketException) { // socket error
        LinkDisconnectedEvent e = new LinkDisconnectedEvent();
        e.Context = asyncState.Session;
        Feed(e);
      }
      catch (ObjectDisposedException) { // socket closed
        LinkDisconnectedEvent e = new LinkDisconnectedEvent();
        e.Context = asyncState.Session;
        Feed(e);
      }
    }

    public void OnSend(IAsyncResult asyncResult) {
      AsyncState asyncState = (AsyncState)asyncResult.AsyncState;
      try {
        int numBytes = asyncState.Session.socket.EndSend(asyncResult);

      }
      catch (SocketException) { // socket error
        LinkDisconnectedEvent e = new LinkDisconnectedEvent();
        e.Context = asyncState.Session;
        Feed(e);
      }
      catch (ObjectDisposedException) { // socket closed
        LinkDisconnectedEvent e = new LinkDisconnectedEvent();
        e.Context = asyncState.Session;
        Feed(e);
      }
    }

    protected override void SetUp() {
      Session.receiveCallback = new AsyncCallback(this.OnReceive);
      Session.sendCallback = new AsyncCallback(this.OnSend);
    }

    protected override void TearDown() {
      Close();
    }

    public class Session {
      public static AsyncCallback receiveCallback;
      public static AsyncCallback sendCallback;
      public Socket socket;

      AsyncRxState receiveState;

      public Session() {
        receiveState = new AsyncRxState();
        receiveState.Session = this;
        receiveState.Buffer = new Buffer(12);
      }

      public void BeginReceive(bool beginning) {
        receiveState.beginning = beginning;
        receiveState.ArraySegments.Clear();
        receiveState.Buffer.ListAvailableSegments(receiveState.ArraySegments);
        socket.BeginReceive(receiveState.ArraySegments, SocketFlags.None, receiveCallback, receiveState);
      }

      public void BeginSend(Buffer buffer) {
        // pre-process
        buffer.Write(sentinel);
        AsyncTxState asyncState = new AsyncTxState();
        asyncState.Session = this;
        asyncState.Buffer = buffer;

        int numLengthBytes = Buffer.WriteUInt29(asyncState.lengthBytes, buffer.Length);
        asyncState.length = buffer.Length + numLengthBytes;
        asyncState.ArraySegments.Add(new ArraySegment<byte>(asyncState.lengthBytes, 0, numLengthBytes));

        buffer.ListOccupiedSegments(asyncState.ArraySegments);
        socket.BeginSend(asyncState.ArraySegments, SocketFlags.None, sendCallback, asyncState);
      }
    }

    protected class AsyncState {
      public Session Session;
      public Buffer Buffer;
      public IList<ArraySegment<byte>> ArraySegments;
      public int length;

      public AsyncState() {
        ArraySegments = new List<ArraySegment<byte>>();
      }
    }

    protected class AsyncRxState : AsyncState {
      public bool beginning = true;
    }

    protected class AsyncTxState : AsyncState {
      public byte[] lengthBytes = new byte[4];
    }
  }
}
