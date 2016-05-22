using System;
using System.ComponentModel.Composition;

namespace CredoLab.ExtractorContract.ExtractorInfo
{
    public interface IExtractorInfo
    {
       
    }
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class ExtractorMetadataAttribute : Attribute, IExtractorInfo
    {

        public ExtractorMetadataAttribute()
        {
            //typeof(IExtractorInfo).
        }

    }
}