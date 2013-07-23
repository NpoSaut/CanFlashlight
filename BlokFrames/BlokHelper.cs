using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace BlokFrames
{
    public static class BlokHelper
    {
        /// <summary>
        /// Выбирает из последовательности CAN-сообщений в БЛОК-сообщения указанного типа
        /// </summary>
        /// <typeparam name="T">Тип сообщений БЛОК</typeparam>
        /// <param name="FramesFlow">Поток CAN-сообдений</param>
        /// <returns>Последовательность сообщений БЛОК указанного типа</returns>
        public static IEnumerable<T> OfFrameType<T>(this IEnumerable<CanFrame> FramesFlow)
            where T: BlokFrame, new()
        {
            var FilteringDescriptors = new HashSet<int>(BlokFrame.GetDescriptors<T>().Values);
            return FramesFlow.Where(f => FilteringDescriptors.Contains(f.Descriptor)).Select(f => BlokFrame.GetBlokFrame<T>(f));
        }
    }
}
