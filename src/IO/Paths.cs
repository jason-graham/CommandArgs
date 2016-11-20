//---------------------------------------------------------------------------- 
//
//  Copyright (C) CSharp Labs.  All rights reserved.
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// 
// History
//  05/13/13    Created 
//
//---------------------------------------------------------------------------

namespace System.IO
{
    public static partial class Paths
    {
        /// <summary>
        /// Determines if two paths are pointing to the same location.
        /// </summary>
        /// <param name="path1">The first path.</param>
        /// <param name="path2">The second path.</param>
        /// <returns>true if the paths are pointing to the same location; otherwise, false.</returns>
        public static bool FilePathsEqual(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1))
                throw new ArgumentNullException("path1");

            if (string.IsNullOrEmpty(path2))
                throw new ArgumentNullException("path2");

            NormalizeDirectory(ref path1);
            NormalizeDirectory(ref path2);

            return string.Equals(path1, path2, StringComparison.OrdinalIgnoreCase); //compare with case insensitive comparer
        }

        private static void NormalizeDirectory(ref string dir)
        {
            if (dir.Length > 3)
            {
                while (dir[dir.Length - 1] == Path.DirectorySeparatorChar) //remove trailing separators
                    dir = dir.Substring(0, dir.Length - 1);
            }

            //get absolute paths for comparison
            dir = Path.GetFullPath(dir);
        }
    }
}