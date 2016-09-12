/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
  rimraf = require("rimraf"),
    minifyCss = require('gulp-minify-css'),
    rename = require("gulp-rename"),
    concat = require('gulp-concat'),
    uglify = require('gulp-uglify'),
    sass = require("gulp-sass");
  

gulp.task("clean:js", function (cb) {
  rimraf(paths.concatJsDest, cb);
});

gulp.task("clean", ["clean:js"]);

var project = {
  webroot: "./wwwroot"
};

var paths = {
  bower: "./bower_components/",
  lib: "./" + project.webroot + "/lib/",
  app: "./" + project.webroot + "/js/",
  dist: "./" + project.webroot + "/dist/",
  sassSource: "./" + project.webroot +  "/lib/**/*.scss",
  cssOutput:  "./" + project.webroot +  "/css"
};

gulp.task("sass", function() {
  gulp.src(paths.sassSource)
      .pipe(sass().on('error', sass.logError))
      .pipe(gulp.dest(paths.cssOutput));
  });

gulp.task("bundleSite", function () {

  return gulp.src([
    paths.app + "site.js",
    paths.app + "app.js",
    paths.app + "import.js"])
  .pipe(concat("all.js"))
  .pipe(gulp.dest(paths.dist))
  .pipe(rename("all.min.js"))
  .pipe(uglify())
  .pipe(gulp.dest(paths.app));

});

gulp.task('jquery', function () {
  return gulp.src([
          paths.lib + "jquery/dist/**/*.js",
  ])
      .pipe(concat('jquery.min.js'))
      .pipe(uglify())
      .pipe(gulp.dest("./wwwroot/js/"));
});

gulp.task('jquery-addons', function () {
  return gulp.src([
          paths.lib + "jquery-validation/dist/jquery.validate.js",
          paths.lib + "jquery-validation-unobtrusive/jquery.validate.unobtrusive.js"
  ])
      .pipe(concat('jquery-addons.min.js'))
      .pipe(uglify())
      .pipe(gulp.dest("./wwwroot/js/"));
});

gulp.task('bootstrapJs', function () {
  return gulp.src(paths.lib + 'bootstrap/dist/js/bootstrap.js')
      .pipe(concat('bootstrap.min.js'))
      .pipe(uglify())
      .pipe(gulp.dest("./wwwroot/js/"));
});

gulp.task('toastr', function () {
  return gulp.src(paths.lib + "toastr/toastr.js")
      .pipe(gulp.dest("./wwwroot/js/"))
      .pipe(uglify())
      .pipe(rename({ extname: '.min.js' }))
      .pipe(gulp.dest("./wwwroot/js/"));
});

gulp.task('fileupload', function () {
  return gulp.src([paths.lib + "blueimp-file-upload/js/vendor/jquery.ui.widget.js",
      paths.lib + "blueimp-file-upload/js/jquery.fileupload.js",
      paths.lib + "blueimp-file-upload/js/jquery.fileupload-process.js"])
      .pipe(concat('fileupload.js'))
      .pipe(gulp.dest("./wwwroot/js/"))
      .pipe(uglify())
      .pipe(rename({ extname: '.min.js' }))
      .pipe(gulp.dest("./wwwroot/js/"));
});


gulp.task('allJs', ['jquery', 'jquery-addons', 'bootstrapJs', 'toastr', 'fileupload', 'bundleSite']);

gulp.task("default", ['allJs', 'sass']);