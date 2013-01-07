;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Ver   Date       Author     Changes
;; 001   2012-12-17 kphuanghk  Initialize the file
;;                             Complete the find-component-view
;; 002   2012-12-12 kphuanghk  Add find Rails Model, update key binding
;;
;;
;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Define Variables
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; (defvar R_APP_ROOT "")
;; (defvar R_VIEWS "")
;; (defvar R_CONTROLLERS "")
;; (defvar R_MODELS "")


(defun hkgs-init (rails_app_path)
"Initialize Rails app, views, models and controllers path.
RAILS_APP_PATH is the root path of the application folder.
"
  (progn (setq R_APP_ROOT rails_app_path)
	 (setq R_VIEWS (concat R_APP_ROOT "/app/views"))
	 (setq R_CONTROLLERS (concat R_APP_ROOT "/app/controllers"))
	 (setq R_MODELS (concat R_APP_ROOT "/app/models"))))

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
    (hkgs-get-view-file comp-view-selected)))


(defun hkgs-find-view-mode2 ()
  "Find the view of Rails web component. In the future, it will support
two modes of finding Rails view:
Mode 1: Select controller name, then select its views.
  e.g. Step 1: home   Step 2: index create destory edit
Mode 2: List controller#view, 
  e.g. home#index home#create home#destory home#edit
" 
  (interactive)
  (let (comps comp-view-selected)
    (setq comps (directory-files R_VIEWS nil "^[^\.]"))
    (setq comp-view-options (pack-comp-list comps))
    (setq comp-view-selected 
	  (completing-read "Find View: " comp-view-options))
    (hkgs-get-view-file comp-view-selected)))

(defun hkgs-find-model ()
  "Find the Rails model and edit."
  (interactive)
  (let (model-selected models)
    (setq models (directory-files R_MODELS nil "rb$"))
    (setq model-selected (completing-read "Find Models: " models))
    (find-file (concat R_MODELS "/" model-selected))))

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


(defun pack-comp-list (comps)
  " Pack Rails components and return list contains comp#file."
  (let (result)
    (dolist (comp comps result)
      (setq result 
	    (append result 
		    (pack-view comp 
			       (directory-files
				(concat R_VIEWS "/" comp) nil "erb$")))))
    result))

;; Unit Testing
;; (pack-comp-list '("home" "events"))

(defun hkgs-view-prefix (value)
  (substring value 0 (string-match "\\." value)))
;; Unit Testing
;; (hkgs-view-prefix "abc.html.erb")

;; retrive view file
;; input as controller#view format
(defun hkgs-get-view-file (view)
  (let (view-fullpath)
    (setq view (split-string view "#"))
    (setq view-fullpath 
	  (concat R_VIEWS "/" (car view) "/" (car (cdr view)) ".html.erb"))
    (find-file view-fullpath)))

;; Key bindings for HKGS Rails Plugin
(global-set-key (kbd "C-c C") 'hkgs-find-controller)
(global-set-key (kbd "C-c V") 'hkgs-find-view-mode2)
(global-set-key (kbd "C-c M") 'hkgs-find-model)
