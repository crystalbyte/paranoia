#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    /// <summary>
    ///   This class handles all property validation.
    ///   Code inspired by http://weblogs.asp.net/marianor/archive/2009/04/17/wpf-validation-with-attributes-and-idataerrorinfo-interface-in-mvvm.aspx.
    /// </summary>
    /// <typeparam name="T"> The inheriting type. </typeparam>
    public abstract class ValidationObject<T> : NotificationObject, IDataErrorInfo where T : ValidationObject<T> {
        #region Private Fields

        private bool _isValid;
        private readonly Dictionary<string, int> _propertyErrors = new Dictionary<string, int>();

        private static readonly Dictionary<string, Func<T, object>> PropertyAccessors =
            typeof (T).GetProperties().Where(p => GetValidations(p).Length != 0).ToDictionary(p => p.Name,
                                                                                              GetValueAccessor);

        // ReSharper disable StaticFieldInGenericType
        private static readonly Dictionary<string, ValidationAttribute[]> Validators =
            typeof (T).GetProperties().Where(p => GetValidations(p).Length != 0).ToDictionary(p => p.Name,
                                                                                              GetValidations);

        // ReSharper restore StaticFieldInGenericType

        #endregion

        public event EventHandler Validating;

        protected virtual void OnValidating(EventArgs e) {
            var handler = Validating;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler Validated;

        protected virtual void OnValidated(EventArgs e) {
            var handler = Validated;
            if (handler != null)
                handler(this, e);
        }

        public bool IsValid {
            get { return _isValid; }
            private set {
                if (_isValid == value) {
                    return;
                }

                RaisePropertyChanging(() => IsValid);
                _isValid = value;
                RaisePropertyChanged(() => IsValid);
                OnValidated(EventArgs.Empty);
            }
        }

        private static ValidationAttribute[] GetValidations(PropertyInfo property) {
            return (ValidationAttribute[]) property.GetCustomAttributes(typeof (ValidationAttribute), true);
        }

        private static Func<T, object> GetValueAccessor(PropertyInfo property) {
            var instance = Expression.Parameter(typeof (T), "i");
            var cast = Expression.TypeAs(
                Expression.Property(instance, property),
                typeof (object));
            return (Func<T, object>) Expression
                                         .Lambda(cast, instance).Compile();
        }

        private static string GetLocalizedErrorMessage(ValidationAttribute a) {
            if (!string.IsNullOrWhiteSpace(a.ErrorMessage)) {
                return a.ErrorMessage;
            }
            return a.ErrorMessageResourceType.
                       GetProperty(a.ErrorMessageResourceName).
                       GetValue(null, BindingFlags.Static, null, null, CultureInfo.CurrentCulture) as string;
        }

        public bool ValidFor<T>(Expression<Func<T>> property) {
            var propertyName = PropertySupport.ExtractPropertyName(property);
            return !_propertyErrors.ContainsKey(propertyName);
        }

        #region Implementation of IDataErrorInfo

        /// <summary>
        /// </summary>
        /// <param name="columnName"> </param>
        /// <returns> </returns>
        public string this[string columnName] {
            get {
                OnValidating(EventArgs.Empty);
                if (PropertyAccessors.ContainsKey(columnName)) {
                    var value = PropertyAccessors[columnName](this as T);
                    var errors = Validators[columnName].Where(v => !v.IsValid(value))
                        .Select(GetLocalizedErrorMessage).ToArray();

                    _propertyErrors[columnName] = errors.Length;
                    IsValid = _propertyErrors.Values.Sum(x => x) < 1;

                    if (IsValid) {
                        _propertyErrors.Clear();
                    }

                    OnValidated(EventArgs.Empty);
                    return string.Join(Environment.NewLine, errors);
                }
                IsValid = false;
                OnValidated(EventArgs.Empty);
                return string.Empty;
            }
        }

        public string Error {
            get { return string.Empty; }
        }

        #endregion
    }
}