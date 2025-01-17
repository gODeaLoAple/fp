using System;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace ResultMonad.Tests
{
    [TestFixture]
    public class ResultTests
    {
        [Test]
        public void Result_Should_Create_Ok()
        {
            var success = Result.Ok(42);
            success.IsSuccess.Should().BeTrue();
            success.GetValueOrThrow().Should().Be(42);
        }

        [Test]
        public void Result_Should_Create_Fail()
        {
            var fail = Result.Fail<int>("123");

            fail.IsSuccess.Should().BeFalse();
            fail.Error.Should().Be("123");
        }

        [Test]
        public void Result_Should_ReturnsFail_FromResultOf_OnException()
        {
            var fail = Result.Of<int>(() => throw new Exception("123"));

            fail.Should().BeEquivalentTo(Result.Fail<int>("123"));
        }

        [Test]
        public void Result_Should_ReturnsFailWithCustomMessage_FromResultOf_OnException()
        {
            var fail = Result.Of<int>(() => throw new Exception("123"), "42");

            fail.Should().BeEquivalentTo(Result.Fail<int>("42"));
        }

        [Test]
        public void Result_Should_ReturnsOk_FromResultOf_WhenNoException()
        {
            var success = Result.Of(() => 42);

            success.Should().BeEquivalentTo(Result.Ok(42));
        }

        [Test]
        public void Result_Should_RunThen_WhenOk()
        {
            var success = Result.Ok(42)
                .Then(n => n + 10);
            success.Should().BeEquivalentTo(Result.Ok(52));
        }

        [Test]
        public void Result_Should_SkipThen_WhenFail()
        {
            var fail = Result.Fail<int>("Error");
            var called = false;
            fail.Then(n =>
            {
                called = true;
                return n;
            });
            called.Should().BeFalse();
        }

        [Test]
        public void Result_Should_Then_ReturnsFail_OnException()
        {
            int Continuation(int n) => throw new Exception("123");
            var fail = Result.Ok(42)
                .Then(Continuation);
            fail.Should().BeEquivalentTo(Result.Fail<int>("123"));
        }

        [Test]
        public void Result_Should_RunOnFail_WhenFail()
        {
            var fail = Result.Fail<int>("Не число");
            var errorHandler = A.Fake<Action<string>>();

            var parseResult = fail.OnFail(errorHandler);

            A.CallTo(() => errorHandler(null)).WithAnyArguments().MustHaveHappened();
            parseResult.Should().BeEquivalentTo(fail);
        }

        [Test]
        public void Result_Should_SkipOnFail_WhenOk()
        {
            var ok = Result.Ok(42);

            var fail = ok.OnFail(_ => { Assert.Fail("Should not be called"); });

            fail.Should().BeEquivalentTo(ok);
        }

        [Test]
        public void Result_Should_RunThen_WhenOk_Scenario()
        {
            var success =
                Result.Ok("1358571172")
                    .Then(int.Parse)
                    .Then(i => Convert.ToString(i, 16))
                    .Then(hex => Guid.Parse(hex + hex + hex + hex));
            success.Should().BeEquivalentTo(Result.Ok(Guid.Parse("50FA26A450FA26A450FA26A450FA26A4")));
        }

        [Test]
        public void Result_Should_RunThen_WhenOk_ComplexScenario()
        {
            var parsed = Result.Ok("1358571172").Then(int.Parse);
            var success = parsed
                .Then(i => Convert.ToString(i, 16))
                .Then(hex => parsed.GetValueOrThrow() + " -> " + Guid.Parse(hex + hex + hex + hex));
            success.Should().BeEquivalentTo(Result.Ok("1358571172 -> 50fa26a4-50fa-26a4-50fa-26a450fa26a4"));
        }

        [Test]
        public void Result_Should_ReplaceError_IfFail()
        {
            Result.Fail<None>("error")
                .ReplaceError(e => "replaced")
                .Should().BeEquivalentTo(Result.Fail<None>("replaced"));
        }

        [Test]
        public void Result_Should_ReplaceError_DoNothing_IfSuccess()
        {
            Result.Ok(42)
                .ReplaceError(e => "replaced")
                .Should().BeEquivalentTo(Result.Ok(42));
        }

        [Test]
        public void Result_Should_ReplaceError_DontReplace_IfCalledBeforeError()
        {
            Result.Ok(42)
                .ReplaceError(e => "replaced")
                .Then(n => Result.Fail<int>("error"))
                .Should().BeEquivalentTo(Result.Fail<int>("error"));
        }

        [Test]
        public void Result_Should_RefineError_AddErrorMessageBeforePreviousErrorText()
        {
            var calculation = Result.Fail<None>("No connection");
            calculation
                .RefineError("Posting results to db")
                .Should().BeEquivalentTo(Result.Fail<None>("Posting results to db. No connection"));
        }

        [Test]
        public void Result_Should_ValidateReturnResultWithValue_WhenValidated()
        {
            Result.Ok(42)
                .Validate(_ => true, "Error")
                .Should().BeEquivalentTo(Result.Ok(42));
        }

        [Test]
        public void Result_Should_ValidateReturnError_WhenNotValidated()
        {
            Result.Ok(42)
                .Validate(_ => false, "Error")
                .Should().BeEquivalentTo(Result.Fail<int>("Error"));
        }

        [Test]
        public void Result_Should_ToNoneReturnNone_WhenContainedValue()
        {
            Result.Ok(42)
                .ToNone()
                .Should().BeEquivalentTo(Result.Ok());
        }

        [Test]
        public void Result_Should_ToNoneReturnError_WhenContainedError()
        {
            Result.Fail<int>("Error")
                .ToNone()
                .Should().BeEquivalentTo(Result.Fail<None>("Error"));
        }
    }
}