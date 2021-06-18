using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Represents some options for <see cref="FlacParser"/>
    /// </summary>
    public readonly struct FlacParserOptions : IEquatable<FlacParserOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlacParserOptions"/> struct.
        /// </summary>
        /// <param name="parseCueSheet">if set to <c>true</c> the <see cref="FlacParser"/> parses the cue sheet.</param>
        /// <param name="parseVorbisComment">if set to <c>true</c> the <see cref="FlacParser"/> parses the vorbis comment.</param>
        /// <param name="parsePictures">if set to <c>true</c> the <see cref="FlacParser"/> parses the picture metadata excluding actual picture.</param>
        /// <param name="preserveApplication">if set to <c>true</c> the <see cref="FlacParser"/> preserves the application.</param>
        /// <param name="preservePadding">if set to <c>true</c> the <see cref="FlacParser"/> preserves the padding.</param>
        /// <param name="preserveUnusedMetadata">if set to <c>true</c> the <see cref="FlacParser"/> preserves the unused metadata.</param>
        public FlacParserOptions(bool parseCueSheet, bool parseVorbisComment, bool parsePictures, bool preserveApplication, bool preservePadding, bool preserveUnusedMetadata)
        {
            ParseCueSheet = parseCueSheet;
            ParseVorbisComment = parseVorbisComment;
            ParsePictures = parsePictures;
            PreserveApplication = preserveApplication;
            PreservePadding = preservePadding;
            PreserveUnusedMetadata = preserveUnusedMetadata;
        }

        /// <summary>
        /// Gets a value indicating whether to parse cue sheet.
        /// </summary>
        /// <value>
        ///   <c>true</c> if you want <see cref="FlacParser"/> to parse cue sheet; otherwise, <c>false</c>.
        /// </value>
        public bool ParseCueSheet { get; }

        /// <summary>
        /// Gets a value indicating whether to parse vorbis comment.
        /// </summary>
        /// <value>
        ///   <c>true</c> if you want <see cref="FlacParser"/> to parse vorbis comment; otherwise, <c>false</c>.
        /// </value>
        public bool ParseVorbisComment { get; }

        /// <summary>
        /// Gets a value indicating whether to parse picture metadata <b>EXCLUDING ACTUAL PICTURE</b>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if you want <see cref="FlacParser"/> to parse pictures; otherwise, <c>false</c>.
        /// </value>
        public bool ParsePictures { get; }

        /// <summary>
        /// Gets a value indicating whether to preserve application metadata.
        /// </summary>
        /// <value>
        ///   <c>true</c> if you want <see cref="FlacParser"/> to preserve application metadata; otherwise, <c>false</c>.
        /// </value>
        public bool PreserveApplication { get; }

        /// <summary>
        /// Gets a value indicating whether to preserve padding.
        /// </summary>
        /// <value>
        ///   <c>true</c> if you want <see cref="FlacParser"/> to preserve padding; otherwise, <c>false</c>.
        /// </value>
        public bool PreservePadding { get; }

        /// <summary>
        /// Gets a value indicating whether to preserve unused metadata excluding padding and invalid metadata.
        /// </summary>
        /// <value>
        ///   <c>true</c> if you want <see cref="FlacParser"/> to preserve unused metadata; otherwise, <c>false</c>.
        /// </value>
        public bool PreserveUnusedMetadata { get; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is FlacParserOptions options && Equals(options);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(FlacParserOptions other) => ParseCueSheet == other.ParseCueSheet && ParseVorbisComment == other.ParseVorbisComment && ParsePictures == other.ParsePictures && PreserveApplication == other.PreserveApplication && PreservePadding == other.PreservePadding && PreserveUnusedMetadata == other.PreserveUnusedMetadata;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(ParseCueSheet, ParseVorbisComment, ParsePictures, PreserveApplication, PreservePadding, PreserveUnusedMetadata);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacParserOptions"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacParserOptions"/> to compare.</param>
        /// <param name="right">The second <see cref="FlacParserOptions"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(FlacParserOptions left, FlacParserOptions right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacParserOptions"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacParserOptions"/> to compare.</param>
        /// <param name="right">The second  <see cref="FlacParserOptions"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(FlacParserOptions left, FlacParserOptions right) => !(left == right);
    }
}
