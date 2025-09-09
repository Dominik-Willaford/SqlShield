using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Tests.Model
{
    public abstract class TypeMapBase : IDisposable
    {
        private readonly Func<Type, SqlMapper.ITypeMap>? _originalProvider;

        protected TypeMapBase()
        {
            _originalProvider = SqlMapper.TypeMapProvider;
        }

        public void Dispose()
        {
            SqlMapper.TypeMapProvider = _originalProvider;
        }
    }
}
