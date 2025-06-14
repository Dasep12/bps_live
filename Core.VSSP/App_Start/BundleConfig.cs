using System.Web;
using System.Web.Optimization;

namespace Core.VSSP
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            // SCRIPT BUNDLES *****
            BundleTable.EnableOptimizations = false;

            bundles.Add(new ScriptBundle("~/Core/JS/Plugins").Include(
                    "~/_VSSPCore/lib/jquery/dist/jquery.min.js",
                    "~/_VSSPCore/lib/pace-progress/pace.min.js",
                    "~/_VSSPCore/lib/jquery.counter/waypoints/waypoints.min.js",
                    "~/_VSSPCore/lib/jquery.counter/counterup/counterup.min.js",
                    "~/_VSSPCore/lib/jquery-moment/moment.min.js",
                    "~/_VSSPCore/lib/popper.js/dist/umd/popper.min.js",
                    "~/_VSSPCore/lib/bootstrap/dist/js/bootstrap.min.js",
                    "~/_VSSPCore/lib/bootstrap/dist/js/tempusdominus-bootstrap.min.js",
                    "~/_VSSPCore/lib/bootstrap-select-1.13.09/dist/js/bootstrap-select.min.js",
                    "~/_VSSPCore/lib/selectize/selectize.min.js",
                    "~/_VSSPCore/lib/toastr/toastr.min.js",
                    "~/_VSSPCore/lib/perfect-scrollbar/dist/perfect-scrollbar.min.js",
                    "~/_VSSPCore/lib/jquery-timeselector/timeselector.js",
                    "~/_VSSPCore/lib/full-calendar/full-calendar.js",
                    "~/_VSSPCore/_Index/js/vendor/jquery-1.12.4.min.js",
                    "~/_VSSPCore/lib/bootstrap-datepicker/bootstrap-datepicker.js",
                    "~/_VSSPCore/lib/bootstrap-timepicker/bootstrap-timepicker.min.js",
                    "~/_VSSPCore/lib/@coreui/coreui/dist/js/coreui.min.js"
                    ));

            bundles.Add(new ScriptBundle("~/Core/JS/Validation").Include(
                    "~/_VSSPCore/lib/jquery-validation/dist/jquery.validate.min.js",
                    "~/_VSSPCore/lib/jquery-validation/dist/additional-methods.js"
                    ));

            bundles.Add(new ScriptBundle("~/Core/JS/Chart").Include(
                    "~/_VSSPCore/lib/chart.js/dist/Chart.js",
                    "~/_VSSPCore/lib/@@coreui/coreui-plugin-chartjs-custom-tooltips/dist/js/custom-tooltips.min.js"
                    ));

            //<script src="js/main.js"></script>
            bundles.Add(new ScriptBundle("~/Core/JS/Main").Include(
                    //"~/_VSSPCore/js/main.js",
                    //"~/_VSSPCore/js/charts.js",
                    //"~/_VSSPCore/js/widgets.js",
                    "~/_VSSPCore/js/popovers.js",
                    "~/_VSSPCore/js/colors.js",
                    "~/_VSSPCore/js/base64js.min.js"
                    ));

            bundles.Add(new ScriptBundle("~/Core/JS/MainBottom").Include(
                    "~/_VSSPCore/lib/jquery-ui-1.12.1.custom/jquery-ui-1.13.1.js",
                    "~/_VSSPCore/js/site.js"
                    ));


            bundles.Add(new ScriptBundle("~/Core/JS/jqGrid").Include(
                    "~/_VSSPCore/lib/jqgrid/js/jquery.jqGrid.min.js",
                    "~/_VSSPCore/lib/jqgrid/js/i18n/grid.locale-en.js",
                    "~/_VSSPCore/lib/jqgrid/js/jszip.min.js",
                    "~/_VSSPCore/lib/jqgrid/js/pdfmake.min.js",
                    "~/_VSSPCore/lib/jqgrid/js/vfs_fonts.js"
                    ));

            bundles.Add(new ScriptBundle("~/Core/JS/dataTables").Include(
                    "~/_VSSPCore/lib/datatables/js/jquery.dataTables.min.js",
                    "~/_VSSPCore/lib/datatables/js/dataTables.buttons.min.js"
                    ));

            //<meta charset="utf-8" />
            //<!-- A link to a Bootstrap css -->
            //<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css">     
            //<!-- A link to a Octicons css -->
            //<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/octicons/4.4.0/font/octicons.css">
            //<!-- A link to a jqGrid Bootstrap 4 css -->
            //<link rel="stylesheet" type="text/css" media="screen" href="../../../css/trirand/ui.jqgrid-bootstrap4.css" />

            //<!-- The jQuery library is a prerequisite for all jqSuite products -->
            //<script type="text/ecmascript" src="../../../js/jquery.min.js"></script>

            //<!-- This is the localization file of the grid controlling messages, labels, etc.
            //<!-- We support more than 40 localizations -->
            //<script type="text/ecmascript" src="../../../js/trirand/i18n/grid.locale-en.js"></script>
            //<!-- This is the Javascript file of jqGrid -->   
            //<script type="text/ecmascript" src="../../../js/trirand/jquery.jqGrid.min.js"></script>
            //<!-- Optional Bootstrap JavaScript files -->
            //<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.3/umd/popper.min.js" integrity="sha384-vFJXuSJphROIrBnz7yo7oB41mKfc8JzQZiCq4NCceLEaO4IHwicKwpJf9c9IpFgh" crossorigin="anonymous"></script>
            //<script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js"></script>


            // STYLE BUNDLES *****
            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //          "~/Content/bootstrap.css",
            //          "~/Content/site.css"));

            // COREUI CSS
            bundles.Add(new StyleBundle("~/Core/CSS/Icons").Include(
                  "~/_VSSPCore/lib/toastr/toastr.min.css",
                  "~/_VSSPCore/lib/@coreui/icons/css/coreui-icons.min.css",
                  "~/_VSSPCore/lib/flag-icon-css/css/flag-icon.min.css",
                  "~/_VSSPCore/lib/font-awesome/css/font-awesome.min.css",
                  "~/_VSSPCore/lib/simple-line-icons/css/simple-line-icons.css"
                  ));

            bundles.Add(new StyleBundle("~/Core/CSS/Style").Include(
                  //"~/_VSSPCore/lib/bootstrap/dist/css/bootstrap.min.css", jangan diaktifin, sudah di merge ~/_VSSPCore/css/style.css
                  "~/_VSSPCore/lib/bootstrap/dist/css/tempusdominus-bootstrap4.min.css",
                  "~/_VSSPCore/css/style.css",
                  "~/_VSSPCore/css/timeline.css",
                  "~/_VSSPCore/lib/jquery-timeselector/jquery-timeselector.css",
                  "~/_VSSPCore/lib/full-calendar/full-calendar.css",
                  "~/_VSSPCore/lib/font-awesome/css/awesome-bootstrap-checkbox.css",
                  "~/_VSSPCore/lib/bootstrap-select-1.13.14/dist/css/bootstrap-select.min.css",
                  "~/_VSSPCore/lib/bootstrap-datepicker/bootstrap-datepicker.css",
                  "~/_VSSPCore/lib/bootstrap-timepicker/bootstrap-timepicker.min.css",
                  "~/_VSSPCore/vendors/pace-progress/css/pace.css"
                  ));

            bundles.Add(new StyleBundle("~/Core/CSS/jqGrid").Include(
                  //"~/_VSSPCore/lib/jqgrid/css/octicons.css",
                  //"~/_VSSPCore/lib/jqgrid/css/ui.jqgrid-bootstrap.css"
                  "~/_VSSPCore/lib/jquery-ui-themes-1.12.1/jquery-ui.min.css",
                  "~/_VSSPCore/lib/jqgrid/css/ui.jqgrid.css",
                  "~/_VSSPCore/css/jqgrid_custom.css"
                  ));


            bundles.Add(new StyleBundle("~/Core/CSS/dataTables").Include(
                  "~/_VSSPCore/lib/datatables/css/jquery.dataTables.min.css"
                  ));

            /* Google Font & gTag */
            bundles.UseCdn = true;
            //BundleTable.EnableOptimizations = true; //force optimization while debugging

            //var FontUbuntu = "~/https://fonts.googleapis.com/css?family=Ubuntu:300";
            //var FontFrontEnd = "~/https://fonts.googleapis.com/css?family=Open+Sans:300,300i,400,400i,700,700i|Montserrat:300,400,500,700";
            bundles.Add(new StyleBundle("~/google/fonts").Include(
                                    //FontUbuntu, FontFrontEnd
                                    ));

            var GeoTag = "~/https://www.googletagmanager.com/gtag/js?id=UA-118965717-3";
            bundles.Add(new StyleBundle("~/google/geotag").Include(
                                    GeoTag));

        }
    }
}
