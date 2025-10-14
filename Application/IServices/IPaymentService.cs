﻿using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IPaymentService
    {
        Task<bool> ConfirmAsync(ConfirmPaymentRequest req, CancellationToken ct = default);
    }
}
