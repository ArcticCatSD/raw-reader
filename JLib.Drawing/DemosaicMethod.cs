namespace JLib.Drawing
{
    public enum DemosaicMethod
    {
        /// <summary>
        /// Bilinear.
        /// </summary>
        OpenCV,

        /// <summary>
        /// Bilinear.
        /// </summary>
        Imatest,

        /// <summary>
        /// High-quality linear interpolation.
        /// </summary>
        MATLAB,

        /// <summary>
        /// Bilinear.
        /// </summary>
        LabVIEW,
    }
}
