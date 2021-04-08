/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.ArchivalPackage.Model;
using Abc.Nes.Validators;
using System;
using System.IO;

namespace Abc.Nes.ArchivalPackage.Validators {
    public interface IPackageValidator : IDisposable {
        /// <summary>
        /// Checks if the file is a valid ZIP archive and contains the required directories.
        /// </summary>
        /// <param name="filePath">ZIP file path.</param>
        /// <returns>true if the file is a valid ZIP archive and contains the required directories.</returns>
        bool IsPackageValid(string filePath);
        /// <summary>
        /// Checks if the file is a valid ZIP archive and contains the required directories.
        /// </summary>
        /// <param name="stream">ZIP file stream.</param>
        /// <returns>true if the file is a valid ZIP archive and contains the required directories.</returns>
        bool IsPackageValid(Stream stream);
        /// <summary>
        /// Getting package model from a ZIP file.
        /// </summary>
        /// <param name="filePath">ZIP file path.</param>
        /// <returns>Package model.</returns>
        Package GetPackage(string filePath);
        /// <summary>
        /// Getting package model from a ZIP file stream.
        /// </summary>
        /// <param name="stream">ZIP file stream.</param>
        /// <returns>Package model.</returns>
        Package GetPackage(Stream stream);
        /// <summary>
        /// Getting package model from scratch.
        /// </summary>
        /// <param name="filePath">ZIP file path. The model will not be loaded from the file.</param>
        /// <returns>Package model.</returns>
        Package InitializePackage(string filePath = null);
        /// <summary>
        /// A simple method of validation.
        /// </summary>
        /// <param name="filePath">ZIP file path.</param>
        /// <param name="message">Error message text.</param>
        /// <param name="validateMetdataFiles">Also check the correctness of the metadata files.</param>
        /// <param name="breakOnFirstError">Stop checking on the first error.</param>
        /// <returns>Returns validation success information.</returns>
        bool Validate(string filePath, out string message, bool validateMetdataFiles = false, bool breakOnFirstError = true);
        /// <summary>
        /// A simple method of validation
        /// </summary>
        /// <param name="package">Package model.</param>
        /// <param name="message">Error message text.</param>
        /// <param name="validateMetdataFiles">Also check the correctness of the metadata files.</param>
        /// <param name="breakOnFirstError">Stop checking on the first error.</param>
        /// <returns>Returns validation success information.</returns>
        bool Validate(Package package, out string message, bool validateMetdataFiles = false, bool breakOnFirstError = true);
        /// <summary>
        /// Detailed method of validation.
        /// </summary>
        /// <param name="filePath">ZIP file path.</param>
        /// <param name="validateMetdataFiles">Also check the correctness of the metadata files.</param>
        /// <param name="breakOnFirstError">Stop checking on the first error.</param>
        /// <returns>Returns details about the success of validation.</returns>
        IValidationResult GetValidationResult(string filePath, bool validateMetdataFiles = false, bool breakOnFirstError = true);
        /// <summary>
        /// Detailed method of validation.
        /// </summary>
        /// <param name="package">Package model.</param>
        /// <param name="validateMetdataFiles">Also check the correctness of the metadata files.</param>
        /// <param name="breakOnFirstError">Stop checking on the first error.</param>        
        /// <returns>Returns details about the success of validation</returns>
        IValidationResult GetValidationResult(Package package, bool validateMetdataFiles = false, bool breakOnFirstError = true);
    }
}
