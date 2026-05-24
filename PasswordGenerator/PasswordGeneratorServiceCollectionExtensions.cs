using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PasswordGenerator
{
    /// <summary>
    ///     Opt-in dependency-injection registration for the password generator. Call this from the
    ///     consuming application's startup; the package does not auto-register anything.
    /// </summary>
    public static class PasswordGeneratorServiceCollectionExtensions
    {
        /// <summary>Registers the generator, optionally configuring it in code.</summary>
        public static IServiceCollection AddPasswordGenerator(this IServiceCollection services,
            Action<PasswordOptions>? configure = null)
        {
            var options = new PasswordOptions();
            configure?.Invoke(options);
            return AddCore(services, options);
        }

        /// <summary>Registers the generator, binding options from a configuration section (e.g. appSettings.json).</summary>
        public static IServiceCollection AddPasswordGenerator(this IServiceCollection services,
            IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var options = new PasswordOptions();
            configuration.Bind(options);
            return AddCore(services, options);
        }

        private static IServiceCollection AddCore(IServiceCollection services, PasswordOptions options)
        {
            services.AddSingleton<IRandomSource, CryptoRandomSource>();
            services.AddSingleton<IPasswordGenerator>(sp =>
                CreateGenerator(options, sp.GetRequiredService<IRandomSource>()));
            return services;
        }

        private static Password CreateGenerator(PasswordOptions options, IRandomSource randomSource)
        {
            // Build with the non-special classes first, then layer special characters on (default or
            // custom) so the combined character set is assembled correctly.
            var settings = new PasswordSettings(options.IncludeLowercase, options.IncludeUppercase,
                options.IncludeNumeric, false, options.Length, 10000, usingDefaults: false);

            var password = new Password(settings, randomSource);

            if (options.IncludeSpecial)
            {
                if (!string.IsNullOrEmpty(options.SpecialCharacters))
                    password.IncludeSpecial(options.SpecialCharacters!);
                else
                    password.IncludeSpecial();
            }

            return password;
        }
    }
}
