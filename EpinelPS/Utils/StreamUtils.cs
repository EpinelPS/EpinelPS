namespace EpinelPS.Utils;


/// <summary>
/// This class contain utility functions for stream manipulation
/// </summary>
public class StreamUtils
{
    /// <summary>
    /// Alternative parser for NetCustomPackageSetupData.
    /// At the time of writing, the DataList in the ReqSetCustomPackageSlot request is always empty.
    /// However, the data is present in the request body. This function extracts the content if available.
    /// This only reads the relevant data for the wishlist selection
    /// </summary>
    public static List<NetCustomPackageSetupData> ParseNetCustomPackageSetupData(Stream stream)
    {
        var result = new List<NetCustomPackageSetupData>();
        using var reader = new BinaryReader(stream);

        // Process the protobuf model manually
        // We are going to follow the official definition of the NetCustomPackageSetupData message,
        // but assume that the data_list is field 3 instead of 1;
        while (stream.Position < stream.Length)
        {
            byte tag = reader.ReadByte();
            int fieldNumber = tag >> 3;
            int wireType = tag & 0x07;
            
            // Field 3 (0x1A): repeated NetCustomPackageSetupData data_list (wire type 2)
            if (fieldNumber == 3 && wireType == 2)
            {
                int length = (int)ReadVarInt(reader);
                int endOfMessage = (int)stream.Position + length;

                var data = ParseNetCustomPackageSetupDataFromStream(reader, endOfMessage);
                result.Add(data);

                stream.Position = endOfMessage;
            }
            else
            {
                // Unknown fields, skip
                SkipField(reader, wireType);
            }
        }

        return result;
    }

    private static NetCustomPackageSetupData ParseNetCustomPackageSetupDataFromStream(BinaryReader reader, int endOfMessage)
    {
        var data = new NetCustomPackageSetupData();

        // Process the request stream
        while (reader.BaseStream.Position < endOfMessage)
        {
            byte tag = reader.ReadByte();
            int fieldNumber = tag >> 3;
            int wireType = tag & 0x07;

            // custom_package_shop_tid
            if (fieldNumber == 1 && wireType == 0)
                data.CustomPackageShopTid = (int)ReadVarInt(reader);
            // slot_list
            else if (fieldNumber == 2 && wireType == 0)
                data.SlotList.Add((int)ReadVarInt(reader));
            // Unknown field
            else             
                SkipField(reader, wireType);
        }

        return data;
    }
      
    /// <summary>
    /// Read a varint type (ulong) variable from a stream
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static ulong ReadVarInt(BinaryReader reader)
    {
        ulong result = 0;
        int shift = 0;
        while (true)
        {
            byte b = reader.ReadByte();
            result |= (ulong)(b & 0x7F) << shift;
            if ((b & 0x80) == 0) break;
            shift += 7;
        }
        return result;
    }

    /// <summary>
    /// Skip a "field" base on the wire type. Only handling 0 (int) and 2 (lists) here but more types can be added later on.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="wireType"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void SkipField(BinaryReader reader, int wireType)
    {
        switch (wireType)
        {
            case 0: ReadVarInt(reader); break;
            case 2:
                int len = (int)ReadVarInt(reader);
                reader.BaseStream.Position += len;
                break;
            case 5: reader.ReadUInt32(); break;
            default: throw new InvalidOperationException($"Unsupported wire type: {wireType}");
        }
    }

}
