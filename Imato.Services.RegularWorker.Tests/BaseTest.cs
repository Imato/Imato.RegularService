﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Imato.Services.RegularWorker.Tests
{
    public class BaseTest
    {
        private static IServiceProvider _provider = null!;
        private static IHost _app = null!;

        public BaseTest()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var builder = Host.CreateDefaultBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IConfiguration>(config);
                //services.AddSingleton<WorkersDbContext>();
                services.AddSingleton<DbLogger>();
            });
            builder.ConfigureLogging((_, logging) =>
            {
                logging.AddConsole();
            });
            builder.ConfigureWorkers(null);
            builder.AddWorkers();
            _app = builder.Build();
            _provider = _app.Services.CreateScope().ServiceProvider;
        }

        public T GetRequiredService<T>() where T : class
        {
            return _provider.GetRequiredService<T>();
        }

        public T GetWorker<T>() where T : class
        {
            return _app.GetWorkers(typeof(T).Name).First() as T;
        }

        public MethodInfo GetMethod<T>(string name,
                object[]? parameters = null)
        {
            if (parameters?.Length > 0)
            {
                return typeof(T)
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.Name == name && x.GetParameters().Length == parameters.Length)
                    .FirstOrDefault() ?? throw new NotExistsMethodException<T>(name);
            }

            return typeof(T).GetMethod(name,
                BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new NotExistsMethodException<T>(name);
        }

        public object? GetField<T>(T obj, string name)
        {
            var field = typeof(T).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(obj);
        }
    }
}