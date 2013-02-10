#!/bin/bash

# Ver  Date       Author       Changes
# 001  2013-02-04 kphuanghk    Initialize the file
#

################################################################################
# This script aims to scaffolding the Quiz application, to make it clear as a 
# simple fundamental mock. As it beigns, it works well with Rails 3.2.9.
#
################################################################################


if [ $# -ne 1 ]; then
   echo "Usage: "
   echo "  $0 RAILS_APP_NAME "
   exit -1
fi

RAPP=$1

rails new $RAPP
cd $RAPP

echo "gem 'thin'" >> Gemfile
echo "gem 'bootstrap-sass'" >> Gemfile

bundle install

#Make home's index as root page
sed "/Application/ a\  root :to => 'home#index'" config/routes.rb  > config/routes.rb.tmp
mv -f config/routes.rb.tmp config/routes.rb
rm public/index.html




#Prepare header and footer
cp ../custom.css.scss  app/assets/stylesheets/custom.css.scss
cp ../_lgcfooter app/views/layouts/_lgcfooter.html.erb
cp ../_lgcheader app/views/layouts/_lgcheader.html.erb

sed "/<body>/ a\<%= render 'layouts/lgcheader' %>" app/views/layouts/application.html.erb | \
sed "/yield/ i\<div class='container'>" | \
sed "/yield/ a\<%=render 'layouts/lgcfooter' %>\n<\/div>" > app.tmp
mv -f app.tmp app/views/layouts/application.html.erb


#Generate models
rails g model klasse name:string summary:text year:integer --no-test-framework
rails g model user ename:string class:references --no-test-framework
rails g model course name:string director:string --no-test-framework

sed "/end/ i\  has_many :users" app/models/klasse.rb > klasse.tmp
mv -f klasse.tmp app/models/klasse.rb

#Generate controllers
rails g controller home index --no-test-framework
rails g controller klasse index new update show --no-test-framework

echo "<%= Time.now %>" >> app/views/home/index.html.erb

rake db:create
rake db:migrate

thin start 


