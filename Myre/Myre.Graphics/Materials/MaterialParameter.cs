using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using System.Diagnostics;
using Myre.Extensions;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Materials
{
    struct ParameterType
    {
        public EffectParameterType Type;
        public int Rows;
        public int Columns;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(ParameterType obj)
        {
            return this.Type == obj.Type
                && this.Rows == obj.Rows
                && this.Columns == obj.Columns;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Type.GetHashCode() ^ Rows.GetHashCode() ^ Columns.GetHashCode();
        }
    }

    public class MaterialParameter
    {
        #region Type Mappings
        internal static Dictionary<string, Type> SetterTypeMappings = new Dictionary<string, Type>()
        {
            { typeof(Boolean).Name,   typeof(BooleanMaterialParameterSetter)      },
            { typeof(Texture2D).Name, typeof(Texture2DMaterialParameterSetter)    },
            { typeof(Int32).Name,     typeof(Int32MaterialParameterSetter)        },
            { typeof(Single).Name,    typeof(SingleMaterialParameterSetter)       },
            { typeof(Vector2).Name,   typeof(Vector2MaterialParameterSetter)      },
            { typeof(Vector3).Name,   typeof(Vector3MaterialParameterSetter)      },
            { typeof(Vector4).Name,   typeof(Vector4MaterialParameterSetter)      },
            { typeof(Matrix).Name,    typeof(MatrixMaterialParameterSetter)       },
            { typeof(String).Name,    typeof(StringMaterialParameterSetter)       },
            { typeof(Boolean[]).Name, typeof(BooleanArrayMaterialParameterSetter) },
            { typeof(Int32[]).Name,   typeof(Int32ArrayMaterialParameterSetter)   },
            { typeof(Single[]).Name,  typeof(SingleArrayMaterialParameterSetter)  },
            { typeof(Vector2[]).Name, typeof(Vector2ArrayMaterialParameterSetter) },
            { typeof(Vector3[]).Name, typeof(Vector3ArrayMaterialParameterSetter) },
            { typeof(Vector4[]).Name, typeof(Vector4ArrayMaterialParameterSetter) },
            { typeof(Matrix[]).Name,  typeof(MatrixArrayMaterialParameterSetter)  },
        };

        internal static Dictionary<ParameterType, Type> ParameterTypeMappings = new Dictionary<ParameterType, Type>()
        {
            { new ParameterType() { Type = EffectParameterType.Bool,    Columns = 1, Rows = 1 },  typeof(Boolean)   },
            { new ParameterType() { Type = EffectParameterType.Texture, Columns = 0, Rows = 0 },  typeof(Texture2D) },
            { new ParameterType() { Type = EffectParameterType.Int32,   Columns = 1, Rows = 1 },  typeof(Int32)     },
            { new ParameterType() { Type = EffectParameterType.Single,  Columns = 1, Rows = 1 },  typeof(Single)    },
            { new ParameterType() { Type = EffectParameterType.Single,  Columns = 2, Rows = 1 },  typeof(Vector2)   },
            { new ParameterType() { Type = EffectParameterType.Single,  Columns = 3, Rows = 1 },  typeof(Vector3)   },
            { new ParameterType() { Type = EffectParameterType.Single,  Columns = 4, Rows = 1 },  typeof(Vector4)   },
            { new ParameterType() { Type = EffectParameterType.Single,  Columns = 4, Rows = 4 },  typeof(Matrix)    },
            { new ParameterType() { Type = EffectParameterType.String,  Columns = 0, Rows = 0 },  typeof(String)    }
        };
        #endregion

        private EffectParameter Parameter;
        private MaterialParameterSetter setter;

        public MaterialParameter(EffectParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");
            if (string.IsNullOrEmpty(parameter.Semantic))
                throw new ArgumentException("Material parameters must have a semantic");

            this.Parameter = parameter;
            this.setter = CreateSetter(parameter);
        }

        public void Apply(BoxedValueStore<string> data)
        {
            if (setter != null)
                setter.Apply(data);
        }

        private static MaterialParameterSetter CreateSetter(EffectParameter parameter)
        {
            if (string.IsNullOrEmpty(parameter.Semantic))
                return null;

            var parameterType = new ParameterType() { Type = parameter.ParameterType, Rows = parameter.RowCount, Columns = parameter.ColumnCount };
            Type type;
            if (!ParameterTypeMappings.TryGetValue(parameterType, out type))
            {
#if WINDOWS
                Trace.TraceWarning("An automatic setter could not be created for the Material parameter \"{0}\", with semantic \"{1}\".", parameter.Name, parameter.Semantic);
#endif
            }

            var typeName = type.Name;

            if (parameter.Elements.Count > 0)
                typeName += "[]";

            Type setterType;
            if (SetterTypeMappings.TryGetValue(typeName, out setterType))
                return Activator.CreateInstance(setterType, parameter) as MaterialParameterSetter;
            else
                return null;
        }
    }

    abstract class MaterialParameterSetter
    {
        protected EffectParameter Parameter;
        protected string Semantic;

        public MaterialParameterSetter(EffectParameter parameter)
        {
            this.Parameter = parameter;
            this.Semantic = parameter.Semantic.ToLower();
        }

        public abstract void Apply(BoxedValueStore<string> globals);
    }

    #region SetterGenerator
    static class ParameterSetterGenerator
    {
        static string classTemplate = @"
    class [ClassPrefix]MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<[Type]> value;
        private BoxedValueStore<string> previousGlobals;

        public [ClassPrefix]MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }";

        static string initialisationTemplate = "{ typeof([Type]).Name, typeof([ClassPrefix]MaterialParameterSetter) },";

        public static string Generate()
        {
            StringBuilder output = new StringBuilder();
            StringBuilder initialisation = new StringBuilder();

            var effectParameterType = typeof(EffectParameter);
            var types = from type in MaterialParameter.ParameterTypeMappings.Values
                        select type.Name;

            var parameters = (from method in effectParameterType.GetMethods()
                              where method.Name == "SetValue"
                              select (from parameter in method.GetParameters()
                                      select parameter.ParameterType.Name))
                              .Flatten();

            var arrays = from type in types
                         let typeArray = type + "[]"
                         where parameters.Contains(typeArray)
                         select typeArray;

            foreach (var type in types.Union(arrays))
            {
                var classDefinition = classTemplate.Replace("[ClassPrefix]", type.Replace("[]", "Array")).Replace("[Type]", type);
                output.AppendLine(classDefinition);

                var initialisationLine = initialisationTemplate.Replace("[ClassPrefix]", type.Replace("[]", "Array")).Replace("[Type]", type);
                initialisation.AppendLine(initialisationLine);
            }

            output.AppendLine(initialisation.ToString());

            return output.ToString();
        }
    }
    #endregion

    #region Setters
    class BooleanMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Boolean> value;
        private BoxedValueStore<string> previousGlobals;

        public BooleanMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class Texture2DMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Texture2D> value;
        private BoxedValueStore<string> previousGlobals;

        public Texture2DMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class Int32MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Int32> value;
        private BoxedValueStore<string> previousGlobals;

        public Int32MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class SingleMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Single> value;
        private BoxedValueStore<string> previousGlobals;

        public SingleMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class Vector2MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector2> value;
        private BoxedValueStore<string> previousGlobals;

        public Vector2MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class Vector3MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector3> value;
        private BoxedValueStore<string> previousGlobals;

        public Vector3MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class Vector4MaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector4> value;
        private BoxedValueStore<string> previousGlobals;

        public Vector4MaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class MatrixMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Matrix> value;
        private BoxedValueStore<string> previousGlobals;

        public MatrixMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class StringMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<String> value;
        private BoxedValueStore<string> previousGlobals;

        public StringMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class BooleanArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Boolean[]> value;
        private BoxedValueStore<string> previousGlobals;

        public BooleanArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class Int32ArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Int32[]> value;
        private BoxedValueStore<string> previousGlobals;

        public Int32ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class SingleArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Single[]> value;
        private BoxedValueStore<string> previousGlobals;

        public SingleArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class Vector2ArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector2[]> value;
        private BoxedValueStore<string> previousGlobals;

        public Vector2ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class Vector3ArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector3[]> value;
        private BoxedValueStore<string> previousGlobals;

        public Vector3ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class Vector4ArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Vector4[]> value;
        private BoxedValueStore<string> previousGlobals;

        public Vector4ArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }

    class MatrixArrayMaterialParameterSetter
        : MaterialParameterSetter
    {
        private Box<Matrix[]> value;
        private BoxedValueStore<string> previousGlobals;

        public MatrixArrayMaterialParameterSetter(EffectParameter parameter)
            : base(parameter)
        {
        }

        public override void Apply(BoxedValueStore<string> globals)
        {
            if (value == null || previousGlobals != globals)
            {
                globals.TryGet(Semantic, out value);
                previousGlobals = globals;
            }

            if (value != null)
                Parameter.SetValue(value.Value);
        }
    }
#endregion
}
