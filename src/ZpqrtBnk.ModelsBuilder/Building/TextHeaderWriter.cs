﻿using System.Text;
using ZpqrtBnk.ModelsBuilder.Api;

namespace ZpqrtBnk.ModelsBuilder.Building
{
    public static class TextHeaderWriter
    {
        /// <summary>
        /// Outputs an "auto-generated" header to a string builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        public static void WriteHeader(StringBuilder sb)
        {
            sb.Append("//------------------------------------------------------------------------------\n");
            sb.Append("// <auto-generated>\n");
            sb.Append("//   This code was generated by a tool.\n");
            sb.Append("//\n");
            sb.AppendFormat("//    ZpqrtBnk.ModelsBuilder v{0}\n", ApiVersion.Current.Version);
            sb.Append("//\n");
            sb.Append("//   Changes to this file will be lost if the code is regenerated.\n");
            sb.Append("// </auto-generated>\n");
            sb.Append("//------------------------------------------------------------------------------\n");
        }
    }
}
