using System;
using System.Collections.Generic;
using System.Text;

namespace Star.Project.Data
{
    public static class OverAll
    {

        public static bool True() => true;
        public static bool False() => false;
        public static T? Null<T>() where T : struct => null;
    }
}
