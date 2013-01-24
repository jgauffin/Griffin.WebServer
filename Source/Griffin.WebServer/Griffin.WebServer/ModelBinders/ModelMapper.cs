using System;
using System.Collections;
using System.Collections.Generic;
using Griffin.Networking.Protocol.Http.Implementation;
using Griffin.WebServer.ValueProviders;

namespace Griffin.WebServer.ModelBinders
{
    public class ModelMapper : IModelBinder
    {
        private List<IModelBinder>  _binders = new List<IModelBinder>();

        public ModelMapper()
        {
            _binders.Add(new PrimitiveModelBinder());
            _binders.Add(new EnumModelBinder());
            _binders.Add(new ArrayModelBinder());
            _binders.Add(new DictionaryModelBinder());
            _binders.Add(new ClassBinder());
        }

        /// <summary>
        /// Remove all binders
        /// </summary>
        public void Clear()
        {
            _binders.Clear();
        }

        public void AddBinder(IModelBinder binder)
        {
            _binders.Add(binder);
        }


        public T Bind<T>(HttpRequest request, string name)
        {
            var provider = new RequestValueProvider(request);

            if (!string.IsNullOrEmpty(name))
            {
                var context = new ModelBinderContext(typeof(T), name, "", provider);
                context.RootBinder = this;
                foreach (var modelBinder in _binders)
                {
                    if (modelBinder.CanBind(context))
                    {
                        return (T)modelBinder.Bind(context);
                    }
                }

                return default(T);
            }
            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException("did not expect IEnumerable implementations without a name in the binder.");

            var model = Activator.CreateInstance(typeof(T));
            foreach (var property in model.GetType().GetProperties())
            {
                var context = new ModelBinderContext(property.PropertyType, property.Name, "", provider);
                context.RootBinder = this;
                foreach (var modelBinder in _binders)
                {
                    if (modelBinder.CanBind(context))
                    {
                        var value = modelBinder.Bind(context);
                        property.SetValue(model, value, null);
                        break;
                    }
                }
            }
            return (T)model;

        }
        
        bool IModelBinder.CanBind(IModelBinderContext context)
        {
            return true;
        }

        object IModelBinder.Bind(IModelBinderContext context)
        {
            foreach (var modelBinder in _binders)
            {
                if (modelBinder.CanBind(context))
                {
                    return modelBinder.Bind(context);
                }
            }

            return context.ModelType.IsClass ? null : Activator.CreateInstance(context.ModelType);
        }
    }
}