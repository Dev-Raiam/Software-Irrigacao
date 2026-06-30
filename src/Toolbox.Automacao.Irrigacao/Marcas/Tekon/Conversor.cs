namespace Toolbox.Automacao.Irrigacao.Marcas.Tekon
{
    public static class Conversor
    {
        public static float ToFloat(ushort high, ushort low)
        {
            if (high == 0 && low == 0)
                return 0f;
            byte[] bytes = new byte[4];

            bytes[0] = (byte)(high >> 8);
            bytes[1] = (byte)(high & 0xFF);

            bytes[2] = (byte)(low >> 8);
            bytes[3] = (byte)(low & 0xFF);

            Array.Reverse(bytes);

            return BitConverter.ToSingle(bytes, 0);
        }
    }
}
