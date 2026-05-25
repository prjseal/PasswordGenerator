using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordGenerator
{
    /// <summary>
    ///     The generation contract for a configured password generator. This is the type to depend on
    ///     when a generator is resolved from dependency injection.
    /// </summary>
    public interface IPasswordGenerator
    {
        /// <summary>Generates a single password. Throws <see cref="System.ArgumentException" /> if the settings are invalid.</summary>
        string Next();

        /// <summary>Tries to generate a single password, returning false (instead of throwing) for invalid settings.</summary>
        bool TryNext(out string? password);

        /// <summary>
        ///     Generates a single password. Generation is CPU-bound and completes synchronously; this overload
        ///     exists for ergonomics and to honour cancellation, not to offload work to another thread. A
        ///     <see cref="ValueTask{TResult}" /> is used because the result is always available synchronously.
        ///     If <paramref name="cancellationToken" /> is already cancelled, the returned task is cancelled.
        /// </summary>
        ValueTask<string> NextAsync(CancellationToken cancellationToken = default);

        /// <summary>Generates the default number of passwords (configurable; one unless overridden).</summary>
        IReadOnlyList<string> Generate();

        /// <summary>Generates <paramref name="count" /> passwords.</summary>
        IReadOnlyList<string> Generate(int count);

        /// <summary>Generates the default number of passwords, observing <paramref name="cancellationToken" />.</summary>
        ValueTask<IReadOnlyList<string>> GenerateAsync(CancellationToken cancellationToken = default);

        /// <summary>Generates <paramref name="count" /> passwords, observing <paramref name="cancellationToken" />.</summary>
        ValueTask<IReadOnlyList<string>> GenerateAsync(int count, CancellationToken cancellationToken = default);

        /// <summary>Estimates the strength, in bits, of the output produced by this generator.</summary>
        double EstimateEntropyBits();
    }
}
