using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DropboxSynchronizer.Interfaces;

namespace DropboxSynchronizer
{
    /// <summary>
    /// Local file cache used to store and retrieve files.
    /// </summary>
    public class LocalFileCache : ILocalFileCache
    {
        #region Private Fields

        private readonly string directory;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileCache"/> class.
        /// </summary>
        public LocalFileCache()
        {
            this.directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Stores the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="content">The content.</param>
        public void StoreFile(string fileName, byte[] content)
        {
            using (var fileStream = new FileStream(Path.Combine(this.directory, fileName), FileMode.Create))
            {
                fileStream.Write(content, 0, content.Length);
            }
        }

        /// <summary>
        /// Retrieves the file and removes it from the store
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The file content of the provided file.</returns>
        public byte[] RetrieveFile(string fileName)
        {
            var fullPath = Path.Combine(this.directory, fileName);
            if (File.Exists(fullPath))
            {
                throw new FileNotFoundException();
            }

            using (var fileStream = File.OpenRead(Path.Combine(this.directory, fileName)))
            {
                var bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, Convert.ToInt32(fileStream.Length));

                return bytes;
            }
        }


        /// <summary>
        /// Get the file names of the currently stored files.
        /// </summary>
        /// <returns>A collection of file names.</returns>
        public IEnumerable<string> GetFileNames()
        {
            var directoryInfo = new DirectoryInfo(this.directory);
            return directoryInfo.GetFiles().Select(fileInfo => fileInfo.Name);
        }

        #endregion
    }
}
