﻿using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomProcessors
{
    [ContentProcessor(DisplayName = "MaterialBakerProcessor")]
    class MaterialBakerProcessor : MaterialProcessor
    {
        public override MaterialContent Process(MaterialContent input, ContentProcessorContext context)
        {
            if (context.Parameters.ContainsKey("Defines")) context.Parameters.Remove("Defines");
            if (input.OpaqueData.ContainsKey("Defines")) context.Parameters.Add("Defines", input.OpaqueData["Defines"]);

            return base.Process(input, context);
        }

        protected override ExternalReference<CompiledEffectContent> BuildEffect(ExternalReference<EffectContent> effect, ContentProcessorContext context)
        {
            OpaqueDataDictionary processorParameters = new OpaqueDataDictionary();

            if (context.Parameters.ContainsKey("Defines")) processorParameters.Add("Defines", context.Parameters["Defines"]);

            return context.BuildAsset<EffectContent, CompiledEffectContent>(effect, typeof(FXBakerProcessor).Name, processorParameters, "EffectImporter", effect.Name);
        }
    }
}
