﻿using System;
using System.IO;

namespace Coop.Network
{
    public class ByteReader
    {
        public readonly BinaryReader Binary;
        private readonly MemoryStream m_Stream;

        public ByteReader(ArraySegment<byte> buffer)
        {
            m_Stream = new MemoryStreamSegment(buffer);
            Binary = new BinaryReader(m_Stream);
        }

        public ByteReader(MemoryStream stream)
        {
            m_Stream = stream;
            Binary = new BinaryReader(m_Stream);
        }

        public long RemainingBytes => m_Stream.Length - m_Stream.Position;

        public byte[] ToArray()
        {
            return m_Stream.ToArray();
        }

        public byte PeekByte()
        {
            byte val = Convert.ToByte(m_Stream.ReadByte());
            m_Stream.Position -= 1;
            return val;
        }
    }
}
