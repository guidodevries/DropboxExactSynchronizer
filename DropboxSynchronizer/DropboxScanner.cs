using System;
using System.IO;

using DropboxSynchronizer.Interfaces;
using Spring.Social.Dropbox.Api;

namespace DropboxSynchronizer
{
    public class DropboxScanner
    {
        #region Private Fields

        private const int Interval = 10000;

        private readonly ITimer timer;
        private readonly IDropbox dropboxService;
        private readonly IFileStore fileStore;

        private string dropBoxFolderToScan;
        private string deltaCursor = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DropboxScanner" /> class.
        /// </summary>
        /// <param name="timer">The timer.</param>
        /// <param name="dropboxService">The dropbox service.</param>
        /// <param name="fileStore">The file cache.</param>
        /// <param name="folderToScan">The folder to scan.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public DropboxScanner(ITimer timer, IDropbox dropboxService, IFileStore fileStore, string folderToScan)
        {
            if (timer == null)
            {
                throw new ArgumentNullException("timer");
            }

            if (dropboxService == null)
            {
                throw new ArgumentNullException("dropboxService");
            }

            if (fileStore == null)
            {
                throw new ArgumentNullException("fileStore");
            }

            this.timer = timer;
            this.timer.TimerTick += (s, e) => this.ScanDirectoryForNewFiles();

            this.dropboxService = dropboxService;
            this.fileStore = fileStore;
            this.dropBoxFolderToScan = !string.IsNullOrWhiteSpace(folderToScan) ? folderToScan : "/";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start scanning for new files.
        /// </summary>
        public void Start()
        {
            this.timer.Start(Interval);
        }

        /// <summary>
        /// Stops scanning for new files.
        /// </summary>
        public void Stop()
        {
            this.timer.Stop();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Scans the dropbox directory for new files.
        /// </summary>
        private void ScanDirectoryForNewFiles()
        {
            this.timer.Stop();

            try
            {
                var delta = this.dropboxService.DeltaAsync(this.deltaCursor).Result;
                this.deltaCursor = delta.Cursor;

                foreach (var entry in delta.Entries)
                {
                    if (this.DoesFileMatchScanCriteria(entry.Metadata))
                    {
                        this.dropboxService.DownloadFileAsync(entry.Path).ContinueWith(
                            task =>
                            {
                                var fileName = Path.GetFileName(task.Result.Metadata.Path);
                                this.fileStore.StoreFile(fileName, task.Result.Content);
                            });
                    }
                }
            }
            finally
            {
                this.timer.Start(Interval);
            }
        }

        /// <summary>
        /// Validates whether the provided file metadata matches the scan criteria.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>True in case the file metadata matches the scan criteria. Otherwise false.</returns>
        private bool DoesFileMatchScanCriteria(Entry entry)
        {
            return !entry.IsDirectory && entry.Path.StartsWith(this.dropBoxFolderToScan);
        }

        #endregion
    }
}
