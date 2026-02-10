using ControllerExChanges.Entity;
using ControllerExChanges.Interfaces;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static ControllerExChanges.Interfaces.IConnector;

namespace ControllerExChanges.Services
{
    public class SecuritiesService : ISecuritiesService
    {
        public SecuritiesService(ControllerLogger logger)
        {
            

            _logger = logger.Logger.ForContext<SecuritiesService>();
        }

        #region Properties ================================================================================

        public ConcurrentDictionary<string, Security> Securities => _securities;

        
        public ConcurrentDictionary<string, string> SecNameKeys => _secNameKeys;

        public ConcurrentDictionary<string, string> SecNameAndClassKeys => _secNameAndClassKeys;


        #endregion

        #region Fields ===================================================================================

        /// <summary>
        /// Словарь бумаг на бирже 
        /// </summary>
        protected ConcurrentDictionary<string, Security> _securities = new ConcurrentDictionary<string, Security>();

        ConcurrentDictionary<string, string> _secNameKeys = new ConcurrentDictionary<string, string>();

        ConcurrentDictionary<string, string> _secNameAndClassKeys = new ConcurrentDictionary<string, string>();

        ILogger _logger;        

        #endregion

        #region Methods ==================================================================================

        /// <summary>
        /// Установить новые бумаги с биржи
        /// </summary>
        /// <param name="securities"></param>
        public void SetSecurities(List<Security> securities) // Refactored
        {
            for (int i = 0; i < securities.Count; i++)
            {
                if (securities[i] == null) continue;

                Security sec = securities[i];

                _securities.AddOrUpdate(securities[i].IsinId, sec, (key, value) => value = sec);

                _secNameKeys.AddOrUpdate(securities[i].Name, securities[i].IsinId, (key, value) => value = securities[i].IsinId);

                _secNameAndClassKeys.AddOrUpdate(securities[i].Name + securities[i].ClassCode, securities[i].IsinId, (key, value) => value = securities[i].IsinId);
            }

            SecuritiesChangeEvent?.Invoke(Securities);

            _logger.Information("{@MethodName}, Securities.Count{Count}" , nameof(GetSecurityForIsinId), _securities.Count);
        }

        /// <summary>
        /// Установить новую бумагу с биржи
        /// </summary>
        /// <param name="security"></param>
        public void SetSecurity(Security security)
        {
            _securities.AddOrUpdate(security.IsinId, security, (key, value) => value = security);

            _secNameKeys.AddOrUpdate(security.Name, security.IsinId, (key, value) => value = security.IsinId);

            _secNameAndClassKeys.AddOrUpdate(security.Name + security.ClassCode, security.IsinId, (key, value) => value = security.IsinId);

            SecuritiesChangeEvent?.Invoke(Securities);

            _logger.Information("{@MethodName}, Securities.Count{Count}", nameof(GetSecurityForIsinId), _securities.Count);
        }

        /// <summary>
        /// Получить бумагу по isinId
        /// </summary>
        /// <param name="isinId"></param>
        /// <returns></returns>
        public Security? GetSecurityForIsinId(string isinId) // Refactored
        {
            if (_securities == null
                || _securities.Count == 0)
            {
                _logger.Verbose("{@MethodName}, _securities.Count == 0 ", nameof(GetSecurityForIsinId));
                return null;
            }

            Security? security = null;

            if (_securities.TryGetValue(isinId, out security))
            {
                _logger.Verbose("{@MethodName}, Security {@Security} ", nameof(GetSecurityForIsinId), security);
            }
            else
            {
                _logger.Verbose("{@MethodName}, security == null ", nameof(GetSecurityForIsinId));
            }

            return security;
        }


        public List<Security> GetSecuritiesList() // Refactored
        {
            List<Security> list = new List<Security>();

            foreach (var val in _securities)
            {
                if (val.Value != null) { list.Add(val.Value); }
            }

            _logger.Verbose("{@MethodName}, securities count{@Count}", nameof(GetSecuritiesList), list.Count);

            return list;
        }

        public Security? GetSecurityFromSecNameAndClass(string secName, string secCode = "")
        {
            string? isinId = null;

            Security? security = null;

            if (secCode != ""
                && _secNameAndClassKeys.TryGetValue(secName + secCode, out isinId))
            {
                if (_securities.TryGetValue(isinId, out security))
                {
                    _logger.Verbose("{@MethodName}, Security {@Security}", nameof(GetSecurityFromSecNameAndClass), security);

                    return security;
                }
            }

            if (_secNameKeys.TryGetValue(secName, out isinId))
            {
                if (_securities.TryGetValue(isinId, out security))
                {
                    if (secCode != ""
                        && security.ClassCode == secCode)
                    {
                        _logger.Verbose("{@MethodName}, Security {@Security}", nameof(GetSecurityFromSecNameAndClass), security);

                        return security;
                    }                    
                }
            }            

            //string key = secName.ToLower();

            foreach (var sec in _secNameKeys)
            {
                if (sec.Key == secName)
                {
                    if (_securities.TryGetValue(sec.Value, out security))
                    {
                        if ((secCode != "" && security.ClassCode == secCode)
                            || secCode == "")
                        {
                            _logger.Verbose("{@MethodName}, Security {@Security}", nameof(GetSecurityFromSecNameAndClass), security);

                            return security;
                        }                           
                    }
                }
            }

            _logger.Verbose("{@MethodName}, Security = null", nameof(GetSecurityFromSecNameAndClass));

            return null;
        }


        #endregion

        #region Events ===========================================================================

        public event IConnector.securitiesChangeEvent? SecuritiesChangeEvent;

        #endregion
    }
}
