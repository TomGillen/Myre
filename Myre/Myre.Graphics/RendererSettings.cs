using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Debugging;
using System.Reflection;
using Myre.Collections;
using System.Diagnostics;

namespace Myre.Graphics
{
    public class RendererSettings
    {
        interface ISetting
        {
            string Name { get; }
            string Description { get; }
        }

        class Setting<T>
            : ISetting
        {
            public T Value
            {
                get { return Target.Value; }
                set { Target.Value = value; }
            }

            public string Name { get; set; }
            public string Description { get; set; }
            public Box<T> Target { get; set; }
        }

        private Renderer renderer;
        private CommandEngine engine;
        private List<ISetting> settings;

        public CommandEngine Engine
        {
            get { return engine; }
        }

        public RendererSettings(Renderer renderer)
        {
            this.renderer = renderer;
            this.settings = new List<ISetting>();
        }

        public Box<T> Add<T>(string name, string description = null, T defaultValue = default(T))
        {
            var box = renderer.Data.Get(name, defaultValue);
            var setting = new Setting<T>()
            {
                Name = name,
                Description = description,
                Target = box,
            };

            settings.Add(setting);

            if (engine != null)
            {
                engine.RemoveOption(name);
                engine.AddOption(setting, "Value", name, description);
            }

            return box;
        }

        public void BindCommandEngine(CommandEngine engine)
        {
            if (this.engine == engine)
                return;

            if (this.engine != null)
            {
                foreach (var item in settings)
                    this.engine.RemoveCommand(item.Name);
            }

            this.engine = engine;
            if (engine != null)
            {
                foreach (var item in settings)
                {
                    engine.RemoveOption(item.Name);
                    engine.AddOption(item, "Value", item.Name, item.Description);
                }
            }
        }
    }
}
