using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DropboxSynchronizer.Interfaces;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Spring.Social.Dropbox.Api;

namespace DropboxSynchronizer.Test
{
    [TestClass]
    public class DropboxScannerTest
    {
        #region Private Fields

        private Mock<ITimer> timerMock;
        private Mock<ILocalFileCache> localFileCacheMock;
        private Mock<IDropbox> dropboxApiMock;

        private DropboxScanner dropboxScanner;

        #endregion

        #region Test Setup

        /// <summary>
        /// Initializes the tests.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.timerMock = new Mock<ITimer>();
            this.localFileCacheMock = new Mock<ILocalFileCache>();
            this.dropboxApiMock = new Mock<IDropbox>();

            var deltaPageStub = new DeltaPage { Entries = new List<DeltaEntry>() };
            this.dropboxApiMock.Setup(d => d.DeltaAsync(It.IsAny<string>())).Returns(Task.FromResult(deltaPageStub));

            this.dropboxScanner = new DropboxScanner(this.timerMock.Object, this.dropboxApiMock.Object, this.localFileCacheMock.Object, string.Empty);
        }

        #endregion

        #region TestMethods

        /// <summary>
        /// Test whether the timer is started when the scanner gets instructed to start.
        /// </summary>
        [TestMethod]
        public void Start1()
        {
            this.dropboxScanner.Start();
            this.timerMock.Verify(t => t.Start(It.IsAny<long>()), Times.Once());
        }

        /// <summary>
        /// Test whether the timer is stopped when the scanner gets instructed to stop.
        /// </summary>
        [TestMethod]
        public void Stop1()
        {
            this.dropboxScanner.Stop();
            this.timerMock.Verify(t => t.Stop(), Times.Once());
        }

        /// <summary>
        /// Test whether the DeltaCursor is used in each next request.
        /// </summary>
        [TestMethod]
        public void ScanOnTimerTick1()
        {
            const string SecondDelta = "secondDelta";

            var deltaPageStub = new DeltaPage { Cursor = SecondDelta, Entries = new List<DeltaEntry>() };
            this.dropboxApiMock.Setup(d => d.DeltaAsync(It.IsAny<string>())).Returns(Task.FromResult(deltaPageStub));

            this.timerMock.Raise(t => t.TimerTick += null, new EventArgs());
            this.dropboxApiMock.Verify(d => d.DeltaAsync(null), Times.Once());

            this.timerMock.Raise(t => t.TimerTick += null, new EventArgs());
            this.dropboxApiMock.Verify(d => d.DeltaAsync(SecondDelta), Times.Once());
        }

        /// <summary>
        /// Test whether the timer gets stopped during delta retrieval.
        /// </summary>
        [TestMethod]
        public void ScanOnTimerTick2()
        {
            this.timerMock.Raise(t => t.TimerTick += null, new EventArgs());

            this.timerMock.Verify(t => t.Stop(), Times.Once());
            this.timerMock.Verify(t => t.Start(It.IsAny<long>()), Times.Once());
        }

        /// <summary>
        /// Test whether 3 files are stored when 3 new files are available.
        /// </summary>
        [TestMethod]
        public void ScanOnTimerTick3()
        {
            const string FileName = "test";
            var content = new byte[10];
            var downloadedDropboxFile = new DropboxFile
            {
                Metadata = new Entry { Path = FileName },
                Content = content
            };

            this.dropboxApiMock.Setup(d => d.DeltaAsync(It.IsAny<string>())).Returns(Task.FromResult(this.SetupDeltaPageWith3Files()));

            this.dropboxApiMock.Setup(d => d.DownloadFileAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(downloadedDropboxFile));

            this.timerMock.Raise(t => t.TimerTick += null, new EventArgs());

            this.localFileCacheMock.Verify(c => c.StoreFile(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Exactly(3));
        }

        /// <summary>
        /// Test whether both the fileName and content of the downloaded file math with the file to be stored.
        /// </summary>
        [TestMethod]
        public void ScanOnTimerTick4()
        {
            const string FileName = "test.txt";
            var path = string.Format(@"/TestFolder/{0}", FileName);
            var content = new byte[10];
            var downloadedDropboxFile = new DropboxFile
            {
                Metadata = new Entry { Path = path },
                Content = content
            };

            this.dropboxApiMock.Setup(d => d.DeltaAsync(It.IsAny<string>())).Returns(Task.FromResult(this.SetupDeltaPageWith1File(path)));

            this.dropboxApiMock.Setup(d => d.DownloadFileAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(downloadedDropboxFile));

            this.timerMock.Raise(t => t.TimerTick += null, new EventArgs());

            this.localFileCacheMock.Verify(c => c.StoreFile(FileName, content), Times.Once());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Setups a new delta page with 1 file.
        /// </summary>
        /// <param name="path">The path of the single file.</param>
        /// <returns>A new delta page.</returns>
        private DeltaPage SetupDeltaPageWith1File(string path)
        {
            var file1Stub = new Entry { IsDirectory = false, Path = path };

            return new DeltaPage
            {
                Entries = new List<DeltaEntry>
                            {
                                new DeltaEntry { Metadata = file1Stub }
                            }
            };
        }

        /// <summary>
        /// Setups a new delta page with 3 files.
        /// </summary>
        /// <returns>A new delta page with 3 files.</returns>
        private DeltaPage SetupDeltaPageWith3Files()
        {
            var folderStub = new Entry { IsDirectory = true };
            var file1Stub = new Entry { IsDirectory = false, Path = @"/file1.txt" };
            var file2Stub = new Entry { IsDirectory = false, Path = @"/file2.jpg" };
            var file3Stub = new Entry { IsDirectory = false, Path = @"/file3.jpg" };

            return new DeltaPage
            {
                Entries = new List<DeltaEntry>
                            {
                                new DeltaEntry { Metadata = folderStub },
                                new DeltaEntry { Metadata = file1Stub },
                                new DeltaEntry { Metadata = file2Stub },
                                new DeltaEntry { Metadata = file3Stub }
                            }
            };
        }

        #endregion
    }
}
