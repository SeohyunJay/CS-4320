using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using iCAREWebExperiment.Models;

namespace iCAREWebExperiment.Controllers
{
    public class PatientController : Controller
    {
        private iCAREDBWebTestEntities db = new iCAREDBWebTestEntities();

        // GET: Patient/Index - List all patients
        public ActionResult ManagePatient()
        {
            string sqlQuery = "SELECT * FROM PatientRecord";
            var patients = db.Database.SqlQuery<PatientRecord>(sqlQuery).ToList();
            return View("ManagePatient", patients);
        }

        [HttpGet]
        public ActionResult AddPatient()
        {
            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");
            return View("AddPatient");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPatient(PatientRecord patient)
        {
            if (ModelState.IsValid)
            {
                string generatedID = "PAT-" + Guid.NewGuid().ToString();

                string sqlInsert = @"
                    INSERT INTO PatientRecord (patientID, name, address, dateOfBirth, height, weight, bloodGroup, bedID, treatmentArea, geoID)
                    VALUES (@ID, @name, @address, @dateOfBirth, @height, @weight, @bloodGroup, @BedID, @TreatmentArea, @geoID)";

                db.Database.ExecuteSqlCommand(
                    sqlInsert,
                    new SqlParameter("@ID", generatedID),
                    new SqlParameter("@name", patient.name),
                    new SqlParameter("@address", patient.address),
                    new SqlParameter("@dateOfBirth", patient.dateOfBirth),
                    new SqlParameter("@height", patient.height),
                    new SqlParameter("@weight", patient.weight),
                    new SqlParameter("@bloodGroup", patient.bloodGroup),
                    new SqlParameter("@BedID", patient.bedID),
                    new SqlParameter("@TreatmentArea", patient.treatmentArea),
                    new SqlParameter("@geoID", patient.geoID)
                );

                return RedirectToAction("ManagePatient");
            }
            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");
            return View("AddPatient", patient);
        }

        [HttpGet]
        public ActionResult EditPatient(string id)
        {
            string sqlSelect = "SELECT * FROM PatientRecord WHERE patientID = @ID";
            var patient = db.Database.SqlQuery<PatientRecord>(sqlSelect, new SqlParameter("@ID", id)).FirstOrDefault();

            if (patient == null)
            {
                return HttpNotFound();
            }

            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

            return View("EditPatient", patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPatient(PatientRecord patient)
        {
            if (ModelState.IsValid)
            {
                string sqlUpdate = @"
                    UPDATE PatientRecord 
                    SET name = @name, address = @address, dateOfBirth = @dateOfBirth, height = @height, 
                        weight = @weight, bloodGroup = @bloodGroup, bedID = @BedID, treatmentArea = @TreatmentArea, geoID = @geoID
                    WHERE patientID = @ID";

                db.Database.ExecuteSqlCommand(
                    sqlUpdate,
                    new SqlParameter("@ID", patient.patientID),
                    new SqlParameter("@name", patient.name),
                    new SqlParameter("@address", patient.address),
                    new SqlParameter("@dateOfBirth", patient.dateOfBirth),
                    new SqlParameter("@height", patient.height),
                    new SqlParameter("@weight", patient.weight),
                    new SqlParameter("@bloodGroup", patient.bloodGroup),
                    new SqlParameter("@BedID", patient.bedID),
                    new SqlParameter("@TreatmentArea", patient.treatmentArea),
                    new SqlParameter("@geoID", patient.geoID)
                );

                return RedirectToAction("ManagePatient");
            }
            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

            return View("EditPatient", patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePatient(string id)
        {
            string sqlDelete = "DELETE FROM PatientRecord WHERE patientID = @ID";
            db.Database.ExecuteSqlCommand(sqlDelete, new SqlParameter("@ID", id));

            return RedirectToAction("ManagePatient");
        }

        //// GET: Patient/AssignPatient - Display a list of patients for assignment, filtered by location
        //[HttpGet]
        //public ActionResult AssignPatient(string selectedGeoID)
        //{
        //    // Fetch all GeoCodes for filtering dropdown
        //    string geoQuery = "SELECT * FROM GeoCodes";
        //    ViewBag.GeoCodes = db.Database.SqlQuery<GeoCodes>(geoQuery).ToList();

        //    // Fetch patients based on selected location or get all if no filter
        //    List<PatientRecord> patients;
        //    if (string.IsNullOrEmpty(selectedGeoID))
        //    {
        //        string sqlGetAllPatients = "SELECT * FROM PatientRecord";
        //        patients = db.Database.SqlQuery<PatientRecord>(sqlGetAllPatients).ToList();
        //    }
        //    else
        //    {
        //        string sqlGetPatientsByGeoID = "SELECT * FROM PatientRecord WHERE geoID = @geoID";
        //        patients = db.Database.SqlQuery<PatientRecord>(sqlGetPatientsByGeoID, new SqlParameter("@geoID", selectedGeoID)).ToList();
        //    }

        //    ViewBag.SelectedGeoID = selectedGeoID; // Maintain selected location in view
        //    return View(patients);
        //}

        //// POST: Patient/AssignPatient - Assign selected patients to the logged-in worker
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AssignPatient(List<string> selectedPatients)
        //{
        //    if (selectedPatients == null || !selectedPatients.Any())
        //    {
        //        ViewBag.Message = "Please select at least one patient to assign.";
        //        return RedirectToAction("AssignPatient");
        //    }

        //    // Assuming the worker is identified by a session variable (LoggedUserID)
        //    string workerID = Session["LoggedUserID"]?.ToString();
        //    if (workerID == null)
        //    {
        //        return RedirectToAction("Login", "Account"); // Redirect to login if session expired
        //    }

        //    foreach (var patientID in selectedPatients)
        //    {
        //        string sqlInsertAssignment = "INSERT INTO PatientAssignment (workerID, patientID) VALUES (@workerID, @patientID)";
        //        db.Database.ExecuteSqlCommand(
        //            sqlInsertAssignment,
        //            new SqlParameter("@workerID", workerID),
        //            new SqlParameter("@patientID", patientID)
        //        );
        //    }

        //    TempData["SuccessMessage"] = "Patients assigned successfully.";
        //    return RedirectToAction("AssignPatient"); // Redirect back to assignment page
        //}
        // GET: Patient/AssignPatient - Display list of patients for assignment filtered by location
        //[HttpGet]
        //public ActionResult AssignPatient(string selectedGeoID)
        //{
        //    var geoCodes = db.Database.SqlQuery<GeoCodes>("SELECT geoID, description FROM GeoCodes").ToList();
        //    ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

        //    List<PatientRecord> patients;
        //    if (string.IsNullOrEmpty(selectedGeoID))
        //    {
        //        string sqlGetAllPatients = "SELECT * FROM PatientRecord";
        //        patients = db.Database.SqlQuery<PatientRecord>(sqlGetAllPatients).ToList();
        //    }
        //    else
        //    {
        //        string sqlGetPatientsByGeoID = "SELECT * FROM PatientRecord WHERE geoID = @geoID";
        //        patients = db.Database.SqlQuery<PatientRecord>(sqlGetPatientsByGeoID, new SqlParameter("@geoID", selectedGeoID)).ToList();
        //    }

        //    ViewBag.SelectedGeoID = selectedGeoID;
        //    if (TempData["SuccessMessage"] != null) // To display success message if redirected from POST
        //    {
        //        ViewBag.Message = TempData["SuccessMessage"];
        //    }
        //    return View("AssignPatient", patients);
        //}

        // POST: Patient/AssignPatient - Assign selected patients to the logged-in worker

            // GET: Patient/AssignPatient - Display a list of patients for assignment, filtered by location
            [HttpGet]
            public ActionResult AssignPatient(string selectedGeoID)
            {
                // Load geolocation data for dropdown
                var geoCodes = db.Database.SqlQuery<GeoCodes>("SELECT geoID, description FROM GeoCodes").ToList();
                ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

                // Retrieve patients based on selected location, if any
                List<PatientRecord> patients;
                if (string.IsNullOrEmpty(selectedGeoID))
                {
                    string sqlGetAllPatients = "SELECT * FROM PatientRecord";
                    patients = db.Database.SqlQuery<PatientRecord>(sqlGetAllPatients).ToList();
                }
                else
                {
                    string sqlGetPatientsByGeoID = "SELECT * FROM PatientRecord WHERE geoID = @geoID";
                    patients = db.Database.SqlQuery<PatientRecord>(sqlGetPatientsByGeoID, new SqlParameter("@geoID", selectedGeoID)).ToList();
                }

                ViewBag.SelectedGeoID = selectedGeoID;
                return View("AssignPatient", patients);
            }

            // POST: Patient/AssignPatient - Assign the logged-in user to the selected patients
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult AssignPatient(List<string> selectedPatients)
            {
                if (selectedPatients == null || !selectedPatients.Any())
                {
                    TempData["Message"] = "Please select at least one patient to assign.";
                    return RedirectToAction("AssignPatient");
                }

                // Check the role of the logged-in user
                string role = Session["RoleName"]?.ToString();
                if (role == null)
                {
                    return RedirectToAction("Login", "Account"); // Redirect to login if session expired
                }

                bool isUpdated = false;
                foreach (var patientID in selectedPatients)
                {
                    var patient = db.PatientRecord.Find(patientID);
                    if (patient != null)
                    {
                        if (role == "Nurse" && patient.numOfNurses < 3)
                        {
                            // Increment nurse count and update if user is a Nurse and count is < 3
                            patient.numOfNurses += 1;
                            isUpdated = true;
                        }
                        else if (role == "Doctor" && patient.numOfNurses > 0 && !patient.hasDoctor)
                        {
                            // Assign doctor if user is a Doctor and at least one nurse is assigned
                            patient.hasDoctor = true;
                            isUpdated = true;
                        }

                        // Update the database if any changes were made
                        if (isUpdated)
                        {
                            string sqlUpdate = "UPDATE PatientRecord SET numOfNurses = @numOfNurses, hasDoctor = @hasDoctor WHERE patientID = @ID";
                            db.Database.ExecuteSqlCommand(
                                sqlUpdate,
                                new SqlParameter("@numOfNurses", patient.numOfNurses),
                                new SqlParameter("@hasDoctor", patient.hasDoctor),
                                new SqlParameter("@ID", patient.patientID)
                            );
                        }
                    }
                }

                if (isUpdated)
                {
                    TempData["SuccessMessage"] = "Patients assigned successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No patients were assigned. Please check the assignment rules.";
                }

                return RedirectToAction("AssignPatient");
            }

        // GET: Patient/MyBoard - Display the assigned patients for the logged-in worker
        [HttpGet]
        public ActionResult MyBoard()
        {
            string workerID = Session["LoggedUserID"]?.ToString();
            string roleName = Session["RoleName"]?.ToString();

            if (workerID == null || roleName == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Query to get all patients assigned to the logged-in worker
            string sqlQuery = @"
                SELECT p.patientID, p.name, p.address, p.dateOfBirth, p.height, p.weight, 
                       p.bloodGroup, p.bedID, p.treatmentArea, p.numOfNurses, p.hasDoctor
                FROM PatientRecord p
                JOIN TreatmentRecord t ON p.patientID = t.patientID
                WHERE t.workerID = @workerID";

            var assignedPatients = db.Database.SqlQuery<PatientRecord>(sqlQuery,
                new SqlParameter("@workerID", workerID)).ToList();

            ViewBag.RoleName = roleName;
            return View("MyBoard", assignedPatients);
        }
    }

}

