using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.Enums
{
    public enum BodyType
    {
        Slim = 1,  // low strength score, high cardio
        Athletic = 2,  // balanced scores
        Toned = 3,  // moderate strength + cardio
        Muscular = 4,  // high strength score
        Powerlifter = 5   // very high strength, dominant weightlifting
    }
}
