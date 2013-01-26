// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2.Events {
  public sealed class FlowShutdownEvent : Event {
    new private static readonly Tag tag;

    static FlowShutdownEvent() {
      tag = new Tag(Event.tag, typeof(FlowShutdownEvent), 0,
                    (int)BuiltinType.FlowShutdownEvent);
    }

    public FlowShutdownEvent() : base(tag.NumProps) {}

    public override int GetHashCode() {
      return Hash.Update(base.GetHashCode(), tag.TypeId);
    }

    public override int GetHashCode(Fingerprint fingerprint) {
      return Hash.Update(base.GetHashCode(fingerprint), tag.TypeId);
    }

    public override int GetTypeId() {
      return tag.TypeId;
    }

    public override Cell.Tag GetTypeTag() {
      return tag;
    }
  }
}
