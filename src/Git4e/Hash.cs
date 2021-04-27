using System;
using System.Diagnostics;
using System.Linq;
using ProtoBuf;

namespace Git4e
{
    [DebuggerDisplay("{ToString(),nq}")]
    [ProtoContract(SkipConstructor = true)]
    public class Hash
    {
        [ProtoMember(1)]
        public byte[] RawHash { get; }

        public Hash(byte[] rawHash)
        {
            this.RawHash = rawHash;
        }

        public Hash(string hashText)
        {
            if (hashText != null)
            {
                this.RawHash =
                    Enumerable.Range(0, hashText.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hashText.Substring(x, 2), 16))
                    .ToArray();
            }
        }

        public override string ToString() => this.RawHash == null? null : BitConverter.ToString(this.RawHash).Replace("-", "");

        public static implicit operator byte[](Hash hash) => hash.RawHash;
        public static implicit operator Hash(byte[] rawHash) => new Hash(rawHash);
    }
}
