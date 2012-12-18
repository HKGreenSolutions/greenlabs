;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Ver   Date       Author     Changes
;; 001   2012-12-17 kphuanghk  Initialize the file
;;                             Complete the find-component-view
;;
;;
;;
;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Define Variables
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(defun hkgs-init (rails_app_path)
"Initialize Rails app, views, models and controllers path.
RAILS_APP_PATH is the root path of the application folder.
Do not place "/" at the end of the path"
  (progn (setq R_APP_ROOT rails_app_path)
   (setq R_VIEWS (concat R_APP_ROOT "/views"))
	 (setq R_CONTROLLERS (concat R_APP_ROOT "/controllers"))
	 (setq R_MODELS (concat R_APP_ROOT "/models"))))

;; Unit Testing
;; (hkgs-init "C:/RailsInstaller/Learning/demo/app")
;; (message R_APP_ROOT)
;; (message R_MODELS)
;; (message R_CONTROLLERS)
;; (message R_VIEWS)

(defun hkgs-find-view ()
"Find the view of Rails web component. In the future, it will support
two modes of finding Rails view:
Mode 1: Select controller name, then select its views.
  e.g. Step 1: home   Step 2: index create destory edit
Mode 2: List controller#view, 
  e.g. home#index home#create home#destory home#edit
" 
  (interactive)
  (let (comp-selected comp-view-selected)
    (setq comp-selected 
	  (completing-read "Edit Views: "
			   (directory-files R_VIEWS nil "^[^\.]")))
    (setq comp-view-options 
	  (pack-view comp-selected 
		     (directory-files 
		      (concat R_VIEWS "/" comp-selected) nil "erb$")))
    (setq comp-view-selected 
	  (completing-read "View to Edit: " 
			   comp-view-options))
    (message comp-view-options)))

;; Pack view here
(defun pack-view (ctrl view-names)
  (when view-names
    (let (result)
      (setq result 
	    (format "%s#%s" 
		    ctrl 
		    (hkgs-view-prefix (car view-names))))
      (cons result (pack-view ctrl (cdr view-names))))))

;; Unit Testing
;; (pack-view "home" '("index" "create" "edit" "show"))

(defun hkgs-view-prefix (value)
  (substring value 0 (string-match "\\." value)))
;; Unit Testing
;; (hkgs-view-prefix "abc.html.erb")


