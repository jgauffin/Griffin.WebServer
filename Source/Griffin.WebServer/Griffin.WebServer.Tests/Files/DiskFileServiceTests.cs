using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Griffin.WebServer.Files;
using Xunit;

namespace Griffin.WebServer.Tests.Files
{
    public class DiskFileServiceTests : IDisposable
    {
        private readonly string _path;

        public DiskFileServiceTests()
        {
            _path = Path.GetTempFileName().Replace(".tmp", "");
            Directory.CreateDirectory(_path);
            File.WriteAllText(Path.Combine(_path, "temp.txt"), "Hello world");
        }

        public void Dispose()
        {
            Directory.Delete(_path, true);
        }

        [Fact]
        public void can_load_a_file_With_direct_reference()
        {
            var sut = new DiskFileService("/", _path);



            var actual = sut.GetFiles(new Uri("http://localhost/temp.txt"))
                .ToList();

            actual.Should().NotBeEmpty();
        }
    }
}