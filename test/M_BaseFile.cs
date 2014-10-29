using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace test
{
    public class Base_File
    {

        /// <summary>
        /// ID
        /// </summary>		
        private string _id;
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }
        /// <summary>
        /// FolderID
        /// </summary>		
        private string _folderid;
        public string FolderID
        {
            get { return _folderid; }
            set { _folderid = value; }
        }
        /// <summary>
        /// FileName
        /// </summary>		
        private string _filename;
        public string FileName
        {
            get { return _filename; }
            set { _filename = value; }
        }
        /// <summary>
        /// FilePath
        /// </summary>		
        private string _filepath;
        public string FilePath
        {
            get { return _filepath; }
            set { _filepath = value; }
        }
        /// <summary>
        /// Category
        /// </summary>		
        private string _category;
        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }
        /// <summary>
        /// FileContent
        /// </summary>		
        private byte[] _filecontent;
        public byte[] FileContent
        {
            get { return _filecontent; }
            set { _filecontent = value; }
        }
        /// <summary>
        /// ReadCount
        /// </summary>		
        private int _readcount;
        public int ReadCount
        {
            get { return _readcount; }
            set { _readcount = value; }
        }
        /// <summary>
        /// FileSize
        /// </summary>		
        private int _filesize;
        public int FileSize
        {
            get { return _filesize; }
            set { _filesize = value; }
        }
        /// <summary>
        /// Enabled
        /// </summary>		
        private int _enabled;
        public int Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }
        /// <summary>
        /// SortCode
        /// </summary>		
        private string _sortcode;
        public string SortCode
        {
            get { return _sortcode; }
            set { _sortcode = value; }
        }
        /// <summary>
        /// Description
        /// </summary>		
        private string _description;
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        /// <summary>
        /// CreateUserID
        /// </summary>		
        private string _createuserid;
        public string CreateUserID
        {
            get { return _createuserid; }
            set { _createuserid = value; }
        }
        /// <summary>
        /// CreateUserRealname
        /// </summary>		
        private string _createuserrealname;
        public string CreateUserRealname
        {
            get { return _createuserrealname; }
            set { _createuserrealname = value; }
        }
        /// <summary>
        /// CreateDate
        /// </summary>		
        private DateTime _createdate;
        public DateTime CreateDate
        {
            get { return _createdate; }
            set { _createdate = value; }
        }
        /// <summary>
        /// ModifyUserID
        /// </summary>		
        private string _modifyuserid;
        public string ModifyUserID
        {
            get { return _modifyuserid; }
            set { _modifyuserid = value; }
        }
        /// <summary>
        /// ModifyUserRealname
        /// </summary>		
        private string _modifyuserrealname;
        public string ModifyUserRealname
        {
            get { return _modifyuserrealname; }
            set { _modifyuserrealname = value; }
        }
        /// <summary>
        /// ModifyDate
        /// </summary>		
        private DateTime _modifydate;
        public DateTime ModifyDate
        {
            get { return _modifydate; }
            set { _modifydate = value; }
        }

    }
}
