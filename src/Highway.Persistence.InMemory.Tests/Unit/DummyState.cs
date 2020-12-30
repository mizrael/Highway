﻿using Highway.Core;
using System;

namespace Highway.Persistence.InMemory.Tests.Unit
{
    public class DummyState : SagaState
    {
        public DummyState(Guid id, string foo, int bar) : base(id)
        {
            Foo = foo;
            Bar = bar;
        }

        public string Foo { get; }
        public int Bar { get; }

        public static DummyState New() => new DummyState(Guid.NewGuid(), "lorem ipsum", 42);
    }
}