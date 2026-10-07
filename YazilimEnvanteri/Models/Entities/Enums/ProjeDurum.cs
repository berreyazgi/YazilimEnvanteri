using System.ComponentModel.DataAnnotations;

namespace YazilimEnvanteri.Models.Entities.Enums
{
    public enum ProjeDurum
    {
        Analiz = 1,
        Geliştirme,
        // 3 was İnceleme - removed; existing rows moved to Test (migration RemoveIncelemeDurum).
        Test = 4,
        Tamamlanmış,
        Yayında

    }
}
