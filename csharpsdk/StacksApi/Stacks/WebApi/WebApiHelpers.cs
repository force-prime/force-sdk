using StacksForce.Utils;
using System.Numerics;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    static public class WebApiHelpers
    {
        static public async Task<AsyncCallResult<string?>> ReadonlyGetString(this Blockchain chain, string address, string contract, string method, params Clarity.Value[] arguments)
        {
            var result = await ReadonlyGet<Clarity.StringType>(chain, address, contract, method, arguments).ConfigureAwait(false);

            if (result.IsSuccess)
                return result.Data != null ? new AsyncCallResult<string?>(result.Data.Value) : new AsyncCallResult<string?>((string?) null);

            return new AsyncCallResult<string?>(result.Error!);
        }

        static public async Task<AsyncCallResult<BigInteger?>> ReadonlyGetUlong(this Blockchain chain, string address, string contract, string method, params Clarity.Value[] arguments)
        {
            var result = await ReadonlyGet<Clarity.UInteger128>(chain, address, contract, method, arguments).ConfigureAwait(false);

            if (result.IsSuccess)
                return new AsyncCallResult<BigInteger?>(result.Data!.Value);

            return new AsyncCallResult<BigInteger?>(result.Error!);
        }

        static public async Task<AsyncCallResult<T?>> ReadonlyGet<T>(this Blockchain chain, string address, string contract, string method, params Clarity.Value[] arguments) where T: Clarity.Value
        {
            var result = await chain.CallReadOnly(address, contract, method, address, arguments).ConfigureAwait(false);
            if (result.IsSuccess)
                return new AsyncCallResult<T?>(result.Data!.UnwrapUntil<T>());

            return new AsyncCallResult<T?>(result.Error!);
        }
    }
}
