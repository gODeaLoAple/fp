﻿using System;

namespace ResultMonad
{
    public readonly struct Result<T>
    {
        public readonly string Error;
        internal readonly T Value;

        public Result(string error, T value = default)
        {
            Error = error;
            Value = value;
        }

        public static implicit operator Result<T>(T v) => Result.Ok(v);

        public T GetValueOrThrow()
        {
            if (IsSuccess) return Value;
            throw new InvalidOperationException($"No value. Only Error {Error}");
        }

        public bool IsSuccess => Error == null;
    }

    public static class Result
    {
        public static Result<T> AsResult<T>(this T value) => Ok(value);

        public static Result<T> Ok<T>(T value) => new(null, value);

        public static Result<None> Ok() => Ok<None>(null);

        public static Result<T> Fail<T>(string e) => new(e);

        public static Result<T> Of<T>(Func<T> f, string error = null)
        {
            try
            {
                return Ok(f());
            }
            catch (Exception e)
            {
                return Fail<T>(error ?? e.Message);
            }
        }

        public static Result<None> Of(Action f, string error = null)
        {
            try
            {
                f();
                return Ok();
            }
            catch (Exception e)
            {
                return Fail<None>(error ?? e.Message);
            }
        }

        public static Result<TOutput> Then<TInput, TOutput>(this Result<TInput> input,
            Func<TInput, TOutput> continuation) =>
            input.Then(inp => Of(() => continuation(inp)));

        public static Result<TOutput> Then<TInput, TOutput>(this Result<TInput> input,
            Func<TInput, Result<TOutput>> continuation) =>
            input.IsSuccess
                ? continuation(input.Value)
                : Fail<TOutput>(input.Error);

        public static Result<TInput> OnFail<TInput>(this Result<TInput> input, Action<string> handleError)
        {
            if (!input.IsSuccess) handleError(input.Error);
            return input;
        }

        public static Result<TInput> OnSuccess<TInput>(this Result<TInput> input, Action handle)
        {
            if (input.IsSuccess) handle();
            return input;
        }

        public static Result<TInput>
            ReplaceError<TInput>(this Result<TInput> input, Func<string, string> replaceError) =>
            input.IsSuccess ? input : Fail<TInput>(replaceError(input.Error));

        public static Result<TInput> RefineError<TInput>(this Result<TInput> input, string errorMessage) =>
            input.ReplaceError(err => errorMessage + ". " + err);

        public static Result<TInput> Validate<TInput>(this Result<TInput> input,
            Func<TInput, bool> validator,
            string errorMessage) =>
            input.Then(i => validator(i) ? Ok(i) : Fail<TInput>(errorMessage));
    }
}