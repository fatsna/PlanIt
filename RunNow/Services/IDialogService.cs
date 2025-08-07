using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunNow.Services
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title);
        bool ShowConfirmation(string message, string title);
        string ShowCertificateDialog(); // 자격증 선택용
    }

}
