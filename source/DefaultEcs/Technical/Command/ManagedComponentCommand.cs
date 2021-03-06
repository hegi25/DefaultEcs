﻿using System.Collections.Generic;

namespace DefaultEcs.Technical.Command
{
    internal sealed unsafe class ManagedComponentCommand<T>
    {
        public static void WriteComponent(List<object> objects, byte* data, in T component)
        {
            lock (objects)
            {
                *(int*)data = objects.Count;
                objects.Add(component);
            }
        }

        public static int Set(in Entity entity, List<object> objects, byte* memory)
        {
            entity.Set((T)objects[*(int*)memory]);

            return sizeof(int);
        }
    }
}
