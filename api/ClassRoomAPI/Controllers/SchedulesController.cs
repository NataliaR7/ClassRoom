﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ClassRoomAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ClassRoomAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SchedulesController : Controller
    {

        private readonly IMongoCollection<ScheduleDay> schedulesCollection;
        public SchedulesController(IMongoDatabase db)
        {
            schedulesCollection = db.GetCollection<ScheduleDay>("schedules");
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult Get(string startDate, int count)
        {
            var days = new List<ScheduleDay>();
            var parseDate = new List<int>();
            var date = new DateTime();
            try
            {
                parseDate = startDate.Split('-', '/', '\\', '.', '_').Select(e => int.Parse(e)).ToList();
                date = new DateTime(parseDate[0], parseDate[1], parseDate[2]);
            }
            catch(Exception e)
            {
                return UnprocessableEntity("Invalid format of startDate: " + e.Message);
            }
            if(count < 0)
            {
                return UnprocessableEntity("Invalid parameter: count < 0");
            }
            for(var i = 0; i < count; i++)
            {
                var day = schedulesCollection.Find(a => a.Date == date.AddDays(i)).FirstOrDefault();
                if(day != null)
                {
                     days.Add(day);
                }
                else
                {
                    days.Add(new ScheduleDay() { Id = Guid.Empty, Date = date.AddDays(i), Lessons = new List<Lesson>() });
                }
            }
            return new ObjectResult(days);
        }

        [HttpGet("{date}")]
        [Produces("application/json")]
        public IActionResult Get(string date)
        {
            var parseDate = new List<int>();
            //var dateTime = new DateTime(parseDate[0], parseDate[1], parseDate[2]);
            var dateTime = new DateTime();
            try
            {
                parseDate = date.Split('-', '/', '\\', '.', '_').Select(e => int.Parse(e)).ToList();
                dateTime = new DateTime(parseDate[0], parseDate[1], parseDate[2]);
            }
            catch (Exception e)
            {
                return UnprocessableEntity("Invalid format of date: " + e.Message);
            }
            var result = schedulesCollection.Find(a => a.Date == dateTime).FirstOrDefault();
            return new ObjectResult(result);
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     POST /schedules
        ///     {
        ///        createDate: "DateTime"
        ///        startTime: "10:15"
        ///        endTime: "12:00"
        ///        title: "string"
        ///        audience:"string"
        ///        teacher:"string"
        ///        repeatCount: 1(один раз) или 7 (каждую неделю) или 14 (каждые 2 недели) или 30 (каждый месяц)
        ///        type: "lect" или "lab" или "pract"
        ///     }
        /// </remarks>
        [HttpPost]
        [Produces("application/json")]
        public IActionResult Post([FromBody] Lesson value)
        {
            var lesson = new Lesson(value);
            lesson.Id = Guid.NewGuid();
            //if (schedulesCollection.CountDocuments(e => e.Date == lesson.CreateDate) == 0)
            //{
            //    schedulesCollection.InsertOne(new ScheduleDay() { Id = Guid.NewGuid(), Date = lesson.CreateDate, Lessons = new List<Lesson>() });
            //}
            var update = Builders<ScheduleDay>.Update.Push(s => s.Lessons, lesson);
            UpdateAll(lesson, update, true);
            //schedulesCollection.UpdateOne(s => s.Date == lesson.Date, update);
            //var a = schedulesCollection.Find(a => a.Date == lesson.CreateDate.AddDays(7)).FirstOrDefault();
            return Created("/schedules", lesson);
        }

        private void UpdateAll(Lesson lesson, UpdateDefinition<ScheduleDay> update, bool needCreate)
        {
            var date = new DateTime();
            switch (lesson.RepeatCount)
            {
                case 1:
                    {
                        if (needCreate && schedulesCollection.CountDocuments(e => e.Date == lesson.CreateDate) == 0)
                        {
                            schedulesCollection.InsertOne(new ScheduleDay() { Id = Guid.NewGuid(), Date = lesson.CreateDate, Lessons = new List<Lesson>() });
                        }
                        schedulesCollection.UpdateOne(s => s.Date == lesson.CreateDate, update);
                        break;
                    }
                case 7:
                    {
                        for (var i = 0; i < 30; i++)
                        {
                            if (needCreate && schedulesCollection.CountDocuments(e => e.Date == lesson.CreateDate.AddDays(7 * i)) == 0)
                            {
                                schedulesCollection.InsertOne(new ScheduleDay() { Id = Guid.NewGuid(), Date = lesson.CreateDate.AddDays(7 * i), Lessons = new List<Lesson>() });
                            }
                            date = lesson.CreateDate.AddDays(7 * i);
                            schedulesCollection.UpdateOne(s => s.Date == date, update);
                        }
                        break;
                    }
                case 14:
                    {
                        for (var i = 0; i < 15; i++)
                        {
                            if (needCreate && schedulesCollection.CountDocuments(e => e.Date == lesson.CreateDate.AddDays(14 * i)) == 0)
                            {
                                schedulesCollection.InsertOne(new ScheduleDay() { Id = Guid.NewGuid(), Date = lesson.CreateDate.AddDays(14 * i), Lessons = new List<Lesson>() });
                            }
                            date = lesson.CreateDate.AddDays(14 * i);
                            schedulesCollection.UpdateOne(s => s.Date == date, update);
                        }
                        break;
                    }
                case 30:
                    {

                        for (var i = 0; i < 7; i++)
                        {
                            if (needCreate && schedulesCollection.CountDocuments(e => e.Date == lesson.CreateDate.AddMonths(i)) == 0)
                            {
                                schedulesCollection.InsertOne(new ScheduleDay() { Id = Guid.NewGuid(), Date = lesson.CreateDate.AddMonths(i), Lessons = new List<Lesson>() });
                            }
                            date = lesson.CreateDate.AddMonths(i);
                            schedulesCollection.UpdateOne(s => s.Date == date, update);
                        }
                        break;
                    }
            }
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /schedules/{date}/{id}?all={true}
        ///     {
        ///        startTime: "10:15"
        ///        endTime: "12:00"
        ///        title: "string"
        ///        audience:"string"
        ///        teacher:"string"
        ///        type: "lect" или "lab" или "pract"
        ///     }
        /// </remarks>
        [HttpPatch("{date}/{id}")]
        [Produces("application/json")]
        public IActionResult Patch(string date, Guid id, bool all, [FromBody] Lesson value)
        {
            var parseDate = new List<int>();
            //var dateTime = new DateTime(parseDate[0], parseDate[1], parseDate[2]);
            var dateTime = new DateTime();
            try
            {
                parseDate = date.Split('-', '/', '\\', '.', '_').Select(e => int.Parse(e)).ToList();
                dateTime = new DateTime(parseDate[0], parseDate[1], parseDate[2]);
            }
            catch (Exception e)
            {
                return UnprocessableEntity("Invalid format of date: " + e.Message);
            }
            var delete = Builders<ScheduleDay>.Update.PullFilter(d =>d.Lessons, l=>l.Id == id);

            var day = schedulesCollection.Find(n => n.Date == dateTime).FirstOrDefault();
            if (day == null)
            {
                return NotFound("Lesson with this id not found");
            }
            var lesson = day.Lessons.Where(l=>l.Id == id).FirstOrDefault();
            if(lesson == null)
            {
                return NotFound("Lesson with this id not found");
            }
            lesson.Update(value);
            var push = Builders<ScheduleDay>.Update.Push(d => d.Lessons, lesson);
            if (all)
            {
                UpdateAll(lesson, delete, false);
                UpdateAll(lesson, push, false);
            }
            else
            {
                schedulesCollection.UpdateOne(n => n.Date == dateTime, delete);
                schedulesCollection.UpdateOne(n => n.Date == dateTime, push);
            }
            return new ObjectResult(lesson);
        }

        [HttpDelete("{date}/{id}")]
        [Produces("application/json")]
        public IActionResult Delete(Guid id, string date, bool all)
        {
            var parseDate = new List<int>();
            //var dateTime = new DateTime(parseDate[0], parseDate[1], parseDate[2]);
            var dateTime = new DateTime();
            try
            {
                parseDate = date.Split('-', '/', '\\', '.', '_').Select(e => int.Parse(e)).ToList();
                dateTime = new DateTime(parseDate[0], parseDate[1], parseDate[2]);
            }
            catch (Exception e)
            {
                return UnprocessableEntity("Invalid format of date: " + e.Message);
            }

            var day = schedulesCollection.Find(n => n.Date == dateTime).FirstOrDefault();
            if (day == null)
            {
                return NotFound("Lesson with this id not found");
            }
            var lesson = day.Lessons.Where(l => l.Id == id).FirstOrDefault();
            if (lesson == null)
            {
                return NotFound("Lesson with this id not found");
            }
            var delete = Builders<ScheduleDay>.Update.PullFilter(d => d.Lessons, l => l.Id == id);
            if (all)
            {
                UpdateAll(lesson, delete, true);
            }
            else
            {
                schedulesCollection.UpdateOne(n => n.Date == dateTime, delete);
            } 
            return NoContent();
        }
    }


}
