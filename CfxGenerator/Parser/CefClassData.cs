﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser {
    [Serializable()]
    public class CefClassData {
        public string Name;
        public CefConfigData CefConfig = new CefConfigData();
        public List<CefCppFunctionData> Methods = new List<CefCppFunctionData>();
    }
}
