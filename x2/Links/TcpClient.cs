// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Net;
using System.Net.Sockets;

using x2.Events;

namespace x2.Links {
  public class TcpClient : TcpLink {
    public TcpClient() {}

    protected void Connect(IPAddress ip, int port) {
      if (socket != null) {
        throw new InvalidOperationException();
      }
      socket = new Socket(ip.AddressFamily,
        SocketType.Stream, ProtocolType.Tcp);
      socket.BeginConnect(ip, port, this.OnConnect, null);
    }

    protected void Connect(string ip, int port) {
      Connect(IPAddress.Parse(ip), port);
    }

    private void OnConnect(IAsyncResult asyncResult) {
      try {
          socket.EndConnect(asyncResult);

          LinkConnectedEvent e = new LinkConnectedEvent();
          e.Result = asyncResult.IsCompleted;

          if (e.Result)
          {
              Session session = new Session(socket);
              e.Context = session;

              session.BeginReceive(true);
          }

          Feed(e);
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        throw e;
      }
    }
  }
}
