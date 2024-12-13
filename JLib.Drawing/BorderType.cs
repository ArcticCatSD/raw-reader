namespace JLib.Drawing
{
    /// <summary>
    /// Referenced from OpenCV.
    /// </summary>
    public enum BorderType
    {
        /// <summary>
        /// iiiiii|abcdefgh|iiiiiii
        /// </summary>
        Constant,

        /// <summary>
        /// aaaaaa|abcdefgh|hhhhhhh
        /// </summary>
        Replicate,

        /// <summary>
        /// fedcba|abcdefgh|hgfedcb
        /// </summary>
        Reflect,

        /// <summary>
        /// cdefgh|abcdefgh|abcdefg
        /// </summary>
        Wrap,

        /// <summary>
        /// gfedcb|abcdefgh|gfedcba
        /// </summary>
        Reflect101,

        /// <summary>
        /// Do not look outside of ROI
        /// </summary>
        Isolated
    }
}
