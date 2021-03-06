﻿using AntShares.Core;
using AntShares.Cryptography;
using AntShares.IO;
using System.Collections;
using System.IO;
using System.Linq;

namespace AntShares.Network.Payloads
{
    internal class MerkleBlockPayload : BlockBase
    {
        public int TxCount;
        public UInt256[] Hashes;
        public byte[] Flags;

        public override int Size => base.Size + sizeof(int) + Hashes.Length.GetVarSize() + Hashes.Sum(p => p.Size) + Flags.Length.GetVarSize() + Flags.Length;

        public static MerkleBlockPayload Create(Block block, BitArray flags)
        {
            MerkleTree tree = new MerkleTree(block.Transactions.Select(p => p.Hash).ToArray());
            tree.Trim(flags);
            byte[] buffer = new byte[(flags.Length + 7) / 8];
            flags.CopyTo(buffer, 0);
            return new MerkleBlockPayload
            {
                Version = block.Version,
                PrevBlock = block.PrevBlock,
                MerkleRoot = block.MerkleRoot,
                Timestamp = block.Timestamp,
                Height = block.Height,
                Nonce = block.Nonce,
                NextMiner = block.NextMiner,
                Script = block.Script,
                TxCount = block.Transactions.Length,
                Hashes = tree.ToHashArray(),
                Flags = buffer
            };
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            TxCount = (int)reader.ReadVarInt(int.MaxValue);
            Hashes = reader.ReadSerializableArray<UInt256>();
            Flags = reader.ReadVarBytes();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteVarInt(TxCount);
            writer.Write(Hashes);
            writer.WriteVarBytes(Flags);
        }
    }
}
