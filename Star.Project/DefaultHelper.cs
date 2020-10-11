using System;
using System.Collections.Generic;
using System.Text;

namespace Star.Project
{
    public static class DefaultHelper
    {

        public static bool True() => true;
        public static bool False() => false;
        public static T? Null<T>() where T : struct => null;
    }
}
