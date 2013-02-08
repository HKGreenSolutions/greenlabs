#!/bin/bash

# Ver  Date       Author       Changes
# 001  2013-02-04 kphuanghk    Initialize the file
#

if [ $# -ne 1 ]; then
   echo "Usage: "
   echo "  $0 RAILS_APP_NAME "
   exit -1
fi

RAPP=$1

rails new $RAPP
cd $RAPP

rails g model classes name:string summary:text year:integer
rails g controller home index

echo "gem 'thin'" >> Gemfile
echo "gem 'bootstrap-sass'" >> Gemfile

bundle install

sed "/Application/ a\  root :to => 'home#index'" config/routes.rb  > config/routes.rb.tmp

mv -f config/routes.rb.tmp config/routes.rb

echo "<%= Time.now %>" >> app/views/home/index.html.erb

echo "@import 'bootstrap';" >> "app/assets/stylesheets/custom.css.scss"

rm public/index.html

thin start 
