namespace dong_backend.Services
{
    public class GroupIdHelper
    {
        private static readonly char[] Alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly Random RandomGenerator = new Random();

        public static string GenerateGroupId()
        {
            char[] groupId = new char[6];

            for (int i = 0; i < groupId.Length; i++)
            {
                groupId[i] = Alphabets[RandomGenerator.Next(Alphabets.Length)];
            }

            return new string(groupId);
        }
    }
}
