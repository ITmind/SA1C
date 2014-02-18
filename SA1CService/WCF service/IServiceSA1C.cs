using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace SA1CService
{
   	[ServiceContract]
    public interface IService1C
    {
        /// <summary>
        /// Запусть с указанного действия
        /// </summary>
        /// <param name="settingName">Имя настройки</param>
        /// <param name="status">Текущее действие</param>
        [OperationContract(IsOneWay = true)]
        void ContinueProcess(string settingName, Job job);

        /// <summary>
        /// Продолжить передыдущее действие
        /// </summary>
        /// <param name="settingName">Имя настройки</param>
        [OperationContract(IsOneWay = true)]
        void Process(string settingName);
        
        
        /// <summary>
        /// Выгрузка измененеий из 1Сv8
        /// </summary>
        /// <param name="settingsFile">Имя настройки</param>
        /// <returns>описание ошибки или "ok"</returns>
        [OperationContract(IsOneWay = true)]
        //[OperationContract]
        void SaveChanges(string settingName);

        /// <summary>
        /// Загрузка измененеий в 1С
        /// </summary>
        /// <param name="settingsFile">Имя настройки</param>
        /// <returns>описание ошибки или "ok"</returns>
        [OperationContract(IsOneWay = true)]
        void LoadChanges(string settingName);

        [OperationContract(IsOneWay = true)]
        void ExecuteExchange(string settingName, bool isNewExchange = true);

        /// <summary>
        /// Получение текущего статуса работы службы
        /// </summary>
        /// <param name="settingName"></param>
        [OperationContract]
        Status GetCurrentStatus(string settingName);
        
        [OperationContract]
        List<BaseConfig> GetAllSettings();
    }
}