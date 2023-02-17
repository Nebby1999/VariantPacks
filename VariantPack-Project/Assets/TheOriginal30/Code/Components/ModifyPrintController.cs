﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPI.Components;

namespace TO30.Components
{
    public class ModifyPrintController : VariantComponent
    {
        private void Start()
        {
            var printController = CharacterModel.GetComponent<PrintController>();
            if(printController)
            {
                printController.printTime = 25f;
                printController.maxPrintHeight = 100;
            }
        }
    }
}