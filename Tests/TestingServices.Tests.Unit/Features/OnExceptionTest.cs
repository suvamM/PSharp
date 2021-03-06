﻿//-----------------------------------------------------------------------
// <copyright file="OnExceptionTest.cs">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.PSharp.TestingServices.Tests.Unit
{
    public class OnExceptionTest : BaseTest
    {
        class E : Event
        {
            public MachineId Id;

            public E() { }

            public E(MachineId id)
            {
                Id = id;
            }
        }

        class Ex1 : Exception { }
        class Ex2 : Exception { }

        class M1a : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                throw new Ex1();
            }

            protected override bool OnException(string method, Exception ex)
            {
                if(ex is Ex1) { return true; }
                return false;
            }
        }

        class M1b : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventDoAction(typeof(E), nameof(Act))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                this.Raise(new E(this.Id));
            }

            void Act()
            {
                throw new Ex1();
            }

            protected override bool OnException(string method, Exception ex)
            {
                if (ex is Ex1) { return true; }
                return false;
            }
        }

        class M1c : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventDoAction(typeof(E), nameof(Act))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                this.Raise(new E(this.Id));
            }

            async Task Act()
            {
                await Task.Yield();
                throw new Ex1();
            }

            protected override bool OnException(string method, Exception ex)
            {
                if (ex is Ex1) { return true; }
                return false;
            }
        }

        class M1d : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventGotoState(typeof(E), typeof(Done))]
            [OnExit(nameof(InitOnExit))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                this.Raise(new E(this.Id));
            }

            void InitOnExit()
            {
                throw new Ex1();
            }

            class Done : MachineState { }

            protected override bool OnException(string method, Exception ex)
            {
                if (ex is Ex1) { return true; }
                return false;
            }
        }

        class M2 : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                throw new Ex2();
            }

            protected override bool OnException(string method, Exception ex)
            {
                if (ex is Ex1) { return true; }
                return false;
            }
        }

        class M3a : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventDoAction(typeof(E), nameof(Act))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                throw new Ex1();
            }

            void Act()
            {
                this.Assert(false);
            }

            protected override bool OnException(string method, Exception ex)
            {
                if (ex is Ex1)
                {
                    this.Raise(new E(this.Id));
                    return true;
                }
                return false;
            }
        }

        class M3b : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventDoAction(typeof(E), nameof(Act))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                throw new Ex1();
            }

            void Act()
            {
                this.Assert(false);
            }

            protected override bool OnException(string method, Exception ex)
            {
                if (ex is Ex1)
                {
                    this.Send(this.Id, new E(this.Id));
                    return true;
                }
                return false;
            }
        }

        [Fact]
        public void TestExceptionSuppressed1()
        {
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M1a));
            });

            AssertSucceeded(test);
        }

        [Fact]
        public void TestExceptionSuppressed2()
        {
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M1b));
            });

            AssertSucceeded(test);
        }

        [Fact]
        public void TestExceptionSuppressed3()
        {
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M1c));
            });

            AssertSucceeded(test);
        }

        [Fact]
        public void TestExceptionSuppressed4()
        {
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M1c));
            });

            AssertSucceeded(test);
        }

        [Fact]
        public void TestExceptionNotSuppressed()
        {
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M2));
            });

            AssertFailed(test, 1, true);
        }

        [Fact]
        public void TestRaiseOnException()
        {
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M3a));
            });

            AssertFailed(test, 1, true);
        }

        [Fact]
        public void TestSendOnException()
        {
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M3b));
            });

            AssertFailed(test, 1, true);
        }

    }
}
