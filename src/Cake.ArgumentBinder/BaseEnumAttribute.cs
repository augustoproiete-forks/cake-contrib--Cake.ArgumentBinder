﻿//
// Copyright Seth Hendrick 2019-2021.
// Distributed under the MIT License.
// (See accompanying file LICENSE in the root of the repository).
//

using System;
using System.Text;

namespace Cake.ArgumentBinder
{
    /// <summary>
    /// This class helps bind an enum to an argument.  This allows one to limit
    /// the choices a user is able to pass into the property.
    /// </summary>
    /// <remarks>
    /// Any enum can be used that follows the following rules.  If any
    /// of these rules are broken, the attribute will not validate,
    /// and an exception will be thrown when the binding is attempted:
    /// 
    /// - The enum must contain at least one value within it.
    ///   Empty enums are not allowed.
    /// - If <see cref="BaseAttribute.Required"/> is set to false,
    ///   the enum must have a value set to 0.  0 is the default value
    ///   for an enum, and its the only way we can define a default enum value
    ///   with <see cref="Attribute"/> objects.
    /// </remarks>
    public abstract class BaseEnumAttribute : BaseAttribute
    {
        // ---------------- Fields ----------------

        private readonly Type enumType;

        // ---------------- Constructor ----------------

        protected BaseEnumAttribute( Type enumType, string arg ) :
            base( arg )
        {
            if( enumType.IsEnum == false )
            {
                throw new ArgumentException(
                    "Passed in type is not an Enum",
                    nameof( enumType )
                );
            }

            this.enumType = enumType;

            this.DefaultValue = (Enum)Activator.CreateInstance( enumType );
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The default value of the argument.  This is set to the default
        /// value of the enum, which is the value set to 0.
        /// </summary>
        /// <remarks>
        /// You can not set this to a specific value because of how attributes work.
        /// <see cref="Enum"/> is not a compile-time constant, so it can not be set
        /// when creating an attribute.  The best we can do for a default value is
        /// the enum value set to 0.
        /// 
        /// If <see cref="BaseAttribute.Required"/> is set to false, and 
        /// if an enum does not have a value equal to 0, a validation error will happen.
        /// </remarks>
        public Enum DefaultValue { get; private set; }

        protected sealed override object BaseDefaultValue => this.DefaultValue;

        internal sealed override Type BaseType => this.enumType;

        // ---------------- Functions ----------------

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            this.ToString( builder );

            if( this.HasSecretValue == false )
            {
                builder.AppendLine( "\t\tPossible Values:" );
                foreach( Enum e in Enum.GetValues( this.BaseType ) )
                {
                    builder.AppendLine( $"\t\t\t- {e}" );
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Validates this object.  Returns <see cref="string.Empty"/>
        /// if nothing is wrong, otherwise this returns an error message.
        /// </summary>
        internal string TryValidate()
        {
            StringBuilder builder = new StringBuilder();
            if( string.IsNullOrWhiteSpace( this.ArgName ) )
            {
                builder.AppendLine( nameof( this.ArgName ) + " can not be null, empty, or whitespace." );
            }

            Array values = Enum.GetValues( this.enumType );
            if( values.Length == 0 )
            {
                builder.AppendLine(
                    $"There a no values contained within enum type {this.enumType.Name}.  At least one value must be specified."
                );
            }

            if( this.Required == false )
            {
                bool foundDefaultValue = false;
                foreach( Enum value in values )
                {
                    if( value.Equals( this.DefaultValue ) )
                    {
                        foundDefaultValue = true;
                        break;
                    }
                }

                if( foundDefaultValue == false )
                {
                    builder.AppendLine(
                        $"{nameof(Required)} is set to {false}, but there is no value contained within the enum equal to 0, to represent a default value."
                    );
                }
            }

            return builder.ToString();
        }
    }
}
