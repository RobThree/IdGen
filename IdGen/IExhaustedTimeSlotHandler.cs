using System;
using System.Collections.Generic;
using System.Text;

namespace IdGen
{
    public interface IExhaustedTimeSlotHandler
    {
        void HandleExhaustedTimeSlot(long currentStamp, ITimeSource timeSource);
    }

    public class ExhaustedTimeSlotThrower : IExhaustedTimeSlotHandler
    {
        public void HandleExhaustedTimeSlot(long currentStamp, ITimeSource timeSource)
        {
            throw new NotImplementedException();
        }
    }
}
